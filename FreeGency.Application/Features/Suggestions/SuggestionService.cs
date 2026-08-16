
using Microsoft.Extensions.Logging;

namespace FreeGency.Application.Features.Suggestions;

public sealed class SuggestionService : ISuggestionService
{
    private readonly ICurrentUserService _currentUser;
    private readonly SuggestionDocumentBuilder _documentBuilder;
    private readonly ISuggestionIndexingService _indexing;
    private readonly ISuggestionSearchService _search;
    private readonly IEntitlementService _entitlementService;
    private readonly ILogger<SuggestionService> _logger;

    private readonly IDeveloperProfileRepository _developers;
    private readonly ITeamRepository _teams;
    private readonly ITeamMemberRepository _teamMembers;
    private readonly ITeamJobRepository _teamJobs;
    private readonly IProjectRepository _projects;
    private readonly IPortfolioRepository _portfolios;

    public SuggestionService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        SuggestionDocumentBuilder documentBuilder,
        ISuggestionIndexingService indexing,
        ISuggestionSearchService search,
        IEntitlementService entitlementService,
        ILogger<SuggestionService> logger)
    {
        _currentUser = currentUser;
        _documentBuilder = documentBuilder;
        _indexing = indexing;
        _search = search;
        _entitlementService = entitlementService;
        _logger = logger;

        _developers = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        _teams = unitOfWork.Repository<ITeamRepository, Team>();
        _teamMembers = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _teamJobs = unitOfWork.Repository<ITeamJobRepository, TeamJob>();
        _projects = unitOfWork.Repository<IProjectRepository, Project>();
        _portfolios = unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();
    }

    public async Task<ApiResponse<TeamsForMeResponseDto>> SuggestTeamsForMeAsync(
        int topK = 10,
        CancellationToken ct = default)
    {
        var quota = await _entitlementService.CanConsumeAsync(_currentUser.UserId, FeatureType.TeamSuggestions, ct);
        if (!quota.IsAllowed)
            return ApiResponse.Failure<TeamsForMeResponseDto>(quota.ToAppError());

        var sw = Stopwatch.StartNew();
        try
        {
            var profile = await _developers.GetByUserIdWithSkillsAndInterestsAsync(_currentUser.UserId, ct);
            if (profile is null)
                return ApiResponse.Failure<TeamsForMeResponseDto>(
                    AppError.NotFound(nameof(DeveloperProfile), _currentUser.UserId));

            var doc = await MapDeveloperAsync(profile, ct);
            var scored = await _search.SearchTeamJobsForDeveloperAsync(doc, topK, ct);

            var jobIds = ParseDistinctGuids(scored.Select(s => s.Id));
            var jobs = jobIds.Count == 0
                ? []
                : await _teamJobs.Query()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(j => jobIds.Contains(j.Id) && j.Status == TeamJobStatus.open)
                    .Include(j => j.Team)
                    .Include(j => j.TeamJobSkills).ThenInclude(s => s.Skill)
                    .ToListAsync(ct);

            var jobsById = jobs.ToDictionary(j => j.Id);
            var suggestions = new List<SuggestedTeamJobDto>();
            foreach (var item in scored)
            {
                if (!Guid.TryParse(item.Id, out var jobId) || !jobsById.TryGetValue(jobId, out var job))
                    continue;

                suggestions.Add(new SuggestedTeamJobDto
                {
                    TeamId = job.TeamId,
                    TeamName = job.Team?.Name ?? GetMeta(item.Metadata, "teamName"),
                    TeamAverageRating = job.Team?.AverageRating ?? 0,
                    TeamRatingCount = job.Team?.RatingCount ?? 0,
                    JobId = job.Id,
                    JobTitle = job.Title,
                    JobDescription = job.Description,
                    RequiredSkills = job.TeamJobSkills
                        .Select(s => s.Skill.Name)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToList(),
                    FinalScore = item.FinalScore,
                    VectorScore = item.VectorScore,
                    Breakdown = MapBreakdown(item.Breakdown)
                });
            }

            sw.Stop();
            await _entitlementService.ConsumeAsync(_currentUser.UserId, FeatureType.TeamSuggestions, ct);

            return ApiResponse.Success(new TeamsForMeResponseDto
            {
                Suggestions = suggestions,
                Metadata = new SuggestionMetadataDto
                {
                    ReturnedCount = suggestions.Count,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Warnings = suggestions.Count == 0
                        ? ["No open team jobs matched your profile. Try reindexing or enriching your skills."]
                        : []
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestTeamsForMe failed for user {UserId}", _currentUser.UserId);
            return ApiResponse.Failure<TeamsForMeResponseDto>(
                AppError.Validation(DescribeSuggestionFailure(ex)));
        }
    }

    public async Task<ApiResponse<ProjectCandidatesResponseDto>> SuggestCandidatesForProjectAsync(
        Guid projectId,
        int topK = 10,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        // Check if the user has permission to consume the TeamSuggestions feature
        var quota = await _entitlementService.CanConsumeAsync(_currentUser.UserId, FeatureType.TeamSuggestions, ct);
        if (!quota.IsAllowed)
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(quota.ToAppError());

        try
        {
            var project = await _projects.GetByIdWithDetailsAsync(projectId, ct);
            if (project is null)
                return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                    AppError.NotFound(nameof(Project), projectId));

            if (project.ClientId != _currentUser.UserId)
                return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                    AppError.Forbidden("You do not own this project."));

            if (project.Status != ProjectStatus.Open)
                return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                    AppError.Validation("Project must be published (Open) before requesting candidate suggestions."));

            var scored = await _search.SearchCandidatesForProjectAsync(MapProject(project), topK, ct);
            var candidates = await HydrateCandidatesAsync(scored, ct);

            sw.Stop();

            // Consume the TeamSuggestions feature quota for the current user
            await _entitlementService.ConsumeAsync(_currentUser.UserId, FeatureType.TeamSuggestions, ct);

            return ApiResponse.Success(new ProjectCandidatesResponseDto
            {
                ProjectId = projectId,
                Candidates = candidates,
                Metadata = new SuggestionMetadataDto
                {
                    ReturnedCount = candidates.Count,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Warnings = candidates.Count == 0
                        ? ["No candidates matched. Ensure developers/teams are indexed."]
                        : []
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestCandidatesForProject failed for project {ProjectId}", projectId);
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                AppError.Validation(DescribeSuggestionFailure(ex)));
        }
    }

    public async Task<ApiResponse<ProjectCandidatesResponseDto>> SuggestCandidatesForProjectAsSystemAsync(
        Guid projectId,
        Guid clientUserId,
        int topK = 10,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var project = await _projects.GetByIdWithDetailsAsync(projectId, ct);
            if (project is null)
                return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                    AppError.NotFound(nameof(Project), projectId));

            if (project.ClientId != clientUserId)
                return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                    AppError.Forbidden("You do not own this project."));

            if (project.Status != ProjectStatus.Open)
                return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                    AppError.Validation("Project must be published (Open) before requesting candidate suggestions."));

            var scored = await _search.SearchCandidatesForProjectAsync(MapProject(project), topK, ct);
            var candidates = await HydrateCandidatesAsync(scored, ct);

            sw.Stop();
            return ApiResponse.Success(new ProjectCandidatesResponseDto
            {
                ProjectId = projectId,
                Candidates = candidates,
                Metadata = new SuggestionMetadataDto
                {
                    ReturnedCount = candidates.Count,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Warnings = candidates.Count == 0
                        ? ["No candidates matched. Ensure developers/teams are indexed."]
                        : []
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestCandidatesForProjectAsSystem failed for project {ProjectId}", projectId);
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                AppError.Validation(DescribeSuggestionFailure(ex)));
        }
    }

    public async Task<ApiResponse<ReindexResultDto>> ReindexAllAsync(CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting full suggestions reindex.");

        var developerDocs = new List<BuiltSuggestionDocument>();
        var developers = await _developers.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(d => d.UserSkills).ThenInclude(s => s.Skill)
            .Include(d => d.UserSpecialties).ThenInclude(s => s.Specialty)
            .Include(d => d.UserInterests).ThenInclude(i => i.Category)
            .ToListAsync(ct);

        foreach (var developer in developers)
            developerDocs.Add(_documentBuilder.BuildDeveloper(await MapDeveloperAsync(developer, ct)));

        var teamDocs = new List<BuiltSuggestionDocument>();
        var teams = await _teams.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.TeamSkills).ThenInclude(s => s.Skill)
            .Include(t => t.TeamSpecialties).ThenInclude(s => s.Specialty)
            .Include(t => t.TeamCategories).ThenInclude(c => c.Category)
            .Include(t => t.TeamJobs)
            .ToListAsync(ct);

        foreach (var team in teams)
            teamDocs.Add(_documentBuilder.BuildTeam(await MapTeamAsync(team, ct)));

        var jobDocs = new List<BuiltSuggestionDocument>();
        var openJobs = await _teamJobs.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(j => j.Status == TeamJobStatus.open)
            .Include(j => j.Team).ThenInclude(t => t.TeamSkills).ThenInclude(s => s.Skill)
            .Include(j => j.TeamJobSkills).ThenInclude(s => s.Skill)
            .ToListAsync(ct);

        foreach (var job in openJobs)
            jobDocs.Add(_documentBuilder.BuildTeamJob(await MapTeamJobAsync(job, ct)));

        var projectDocs = new List<BuiltSuggestionDocument>();
        var openProjects = await _projects.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Status == ProjectStatus.Open)
            .Include(p => p.Category)
            .Include(p => p.ProjectSkills).ThenInclude(s => s.Skill)
            .Include(p => p.ProjectSpecialties).ThenInclude(s => s.Specialty)
            .ToListAsync(ct);

        foreach (var project in openProjects)
            projectDocs.Add(_documentBuilder.BuildProject(MapProject(project)));

        var allDocs = developerDocs.Concat(teamDocs).Concat(jobDocs).Concat(projectDocs);
        await _indexing.UpsertBatchAsync(allDocs, ct);
        await PruneStaleSuggestionVectorsAsync(ct);

        sw.Stop();
        _logger.LogInformation(
            "Suggestions reindex finished in {Elapsed}ms (devs={Devs}, teams={Teams}, jobs={Jobs}, projects={Projects}).",
            sw.ElapsedMilliseconds,
            developerDocs.Count,
            teamDocs.Count,
            jobDocs.Count,
            projectDocs.Count);

        return ApiResponse.Success(new ReindexResultDto
        {
            DevelopersIndexed = developerDocs.Count,
            TeamsIndexed = teamDocs.Count,
            TeamJobsIndexed = jobDocs.Count,
            ProjectsIndexed = projectDocs.Count,
            ElapsedMs = sw.ElapsedMilliseconds
        });
    }

    public async Task IndexDeveloperAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _developers.GetByUserIdWithSkillsAndInterestsAsync(userId, ct);
        if (profile is null)
            return;

        await _indexing.UpsertAsync(
            _documentBuilder.BuildDeveloper(await MapDeveloperAsync(profile, ct)),
            ct);
    }

    public async Task IndexTeamAsync(Guid teamId, CancellationToken ct = default)
    {
        var team = await _teams.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.TeamSkills).ThenInclude(s => s.Skill)
            .Include(t => t.TeamSpecialties).ThenInclude(s => s.Specialty)
            .Include(t => t.TeamCategories).ThenInclude(c => c.Category)
            .Include(t => t.TeamJobs)
            .FirstOrDefaultAsync(t => t.Id == teamId, ct);

        if (team is null)
            return;

        await _indexing.UpsertAsync(
            _documentBuilder.BuildTeam(await MapTeamAsync(team, ct)),
            ct);
    }

    public async Task IndexTeamJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _teamJobs.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(j => j.Id == jobId)
            .Include(j => j.Team).ThenInclude(t => t.TeamSkills).ThenInclude(s => s.Skill)
            .Include(j => j.TeamJobSkills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(ct);

        if (job is null)
            return;

        if (job.Status != TeamJobStatus.open)
        {
            await _indexing.DeleteAsync(SuggestionCollections.TeamJobs, job.Id.ToString(), ct);
            await IndexTeamAsync(job.TeamId, ct);
            return;
        }

        await _indexing.UpsertAsync(
            _documentBuilder.BuildTeamJob(await MapTeamJobAsync(job, ct)),
            ct);
        await IndexTeamAsync(job.TeamId, ct);
    }

    public async Task RemoveTeamJobAsync(Guid jobId, Guid teamId, CancellationToken ct = default)
    {
        await _indexing.DeleteAsync(SuggestionCollections.TeamJobs, jobId.ToString(), ct);
        await IndexTeamAsync(teamId, ct);
    }

    public async Task IndexProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projects.GetByIdWithDetailsAsync(projectId, ct);
        if (project is null)
            return;

        if (project.Status != ProjectStatus.Open)
        {
            await _indexing.DeleteAsync(SuggestionCollections.Projects, projectId.ToString(), ct);
            return;
        }

        await _indexing.UpsertAsync(_documentBuilder.BuildProject(MapProject(project)), ct);
    }

    public async Task RemoveProjectAsync(Guid projectId, CancellationToken ct = default)
        => await _indexing.DeleteAsync(SuggestionCollections.Projects, projectId.ToString(), ct);

    private async Task PruneStaleSuggestionVectorsAsync(CancellationToken ct)
    {
        var staleJobIds = await _teamJobs.Query()
            .AsNoTracking()
            .Where(j => j.Status != TeamJobStatus.open)
            .Select(j => j.Id)
            .ToListAsync(ct);

        foreach (var id in staleJobIds)
            await _indexing.DeleteAsync(SuggestionCollections.TeamJobs, id.ToString(), ct);

        var staleProjectIds = await _projects.Query()
            .AsNoTracking()
            .Where(p => p.Status != ProjectStatus.Open)
            .Select(p => p.Id)
            .ToListAsync(ct);

        foreach (var id in staleProjectIds)
            await _indexing.DeleteAsync(SuggestionCollections.Projects, id.ToString(), ct);
    }

    private async Task<List<SuggestedCandidateDto>> HydrateCandidatesAsync(
        IReadOnlyList<ScoredSuggestion> scored,
        CancellationToken ct)
    {
        var teamIds = ParseDistinctGuids(
            scored.Where(s => string.Equals(s.CandidateType, "Team", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id));

        var developerIds = ParseDistinctGuids(
            scored.Where(s => !string.Equals(s.CandidateType, "Team", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id));

        var teams = teamIds.Count == 0
            ? []
            : await _teams.Query()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(t => teamIds.Contains(t.Id))
                .Include(t => t.TeamSkills).ThenInclude(s => s.Skill)
                .Include(t => t.TeamSpecialties).ThenInclude(s => s.Specialty)
                .Include(t => t.TeamCategories).ThenInclude(c => c.Category)
                .ToListAsync(ct);

        var developers = developerIds.Count == 0
            ? []
            : await _developers.Query()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(d => developerIds.Contains(d.UserId))
                .Include(d => d.User)
                .Include(d => d.UserSkills).ThenInclude(s => s.Skill)
                .Include(d => d.UserSpecialties).ThenInclude(s => s.Specialty)
                .ToListAsync(ct);

        var portfolios = teamIds.Count == 0 && developerIds.Count == 0
            ? []
            : await _portfolios.Query()
                .AsNoTracking()
                .Where(p =>
                    (p.OwnerTeamId.HasValue && teamIds.Contains(p.OwnerTeamId.Value)) ||
                    (p.OwnerUserId.HasValue && developerIds.Contains(p.OwnerUserId.Value)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);

        var memberCounts = teamIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await _teamMembers.Query()
                .AsNoTracking()
                .Where(m => teamIds.Contains(m.TeamId))
                .GroupBy(m => m.TeamId)
                .Select(g => new { TeamId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TeamId, x => x.Count, ct);

        var completedCounts = teamIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await _projects.GetProjectsQuery()
                .AsNoTracking()
                .Where(p => p.AssignedTeamId.HasValue
                            && teamIds.Contains(p.AssignedTeamId.Value)
                            && p.Status == ProjectStatus.Completed)
                .GroupBy(p => p.AssignedTeamId!.Value)
                .Select(g => new { TeamId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TeamId, x => x.Count, ct);

        var teamsById = teams.ToDictionary(t => t.Id);
        var developersById = developers.ToDictionary(d => d.UserId);

        var teamPortfolios = portfolios
            .Where(p => p.OwnerTeamId.HasValue)
            .GroupBy(p => p.OwnerTeamId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var developerPortfolios = portfolios
            .Where(p => p.OwnerUserId.HasValue)
            .GroupBy(p => p.OwnerUserId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var candidates = new List<SuggestedCandidateDto>();
        foreach (var item in scored)
        {
            if (!Guid.TryParse(item.Id, out var id))
                continue;

            if (string.Equals(item.CandidateType, "Team", StringComparison.OrdinalIgnoreCase))
            {
                if (!teamsById.TryGetValue(id, out var team))
                    continue;

                var teamPortfolio = teamPortfolios.TryGetValue(team.Id, out var list) ? list : [];
                var featured = teamPortfolio.FirstOrDefault(p => p.Visibility == Visibility.Public)
                    ?? teamPortfolio.FirstOrDefault();
                completedCounts.TryGetValue(team.Id, out var completedCount);
                memberCounts.TryGetValue(team.Id, out var memberCount);

                candidates.Add(new SuggestedCandidateDto
                {
                    CandidateType = "Team",
                    Id = team.Id,
                    Name = team.Name,
                    About = team.AboutUs,
                    AvatarUrl = team.Logo,
                    CoverImageUrl = featured?.ImageCover ?? team.Cover,
                    AverageRating = team.AverageRating,
                    RatingCount = team.RatingCount,
                    Skills = team.TeamSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
                    Specialties = team.TeamSpecialties
                        .Select(s => string.IsNullOrWhiteSpace(s.Specialty.NameEn) ? s.Specialty.NameAr : s.Specialty.NameEn)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToList()!,
                    Categories = team.TeamCategories
                        .Select(c => string.IsNullOrWhiteSpace(c.Category.NameEn) ? c.Category.Name : c.Category.NameEn)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToList()!,
                    PortfolioProjectCount = teamPortfolio.Count,
                    CompletedProjectsCount = Math.Max(completedCount, teamPortfolio.Count(p => p.CompletionDate.HasValue)),
                    MemberCount = memberCount,
                    FeaturedPortfolioProjectId = featured?.Id,
                    FeaturedPortfolioTitle = featured?.Title,
                    FinalScore = item.FinalScore,
                    VectorScore = item.VectorScore,
                    Breakdown = MapBreakdown(item.Breakdown)
                });
            }
            else
            {
                if (!developersById.TryGetValue(id, out var developer))
                    continue;

                var developerPortfolio = developerPortfolios.TryGetValue(developer.UserId, out var list) ? list : [];
                var featured = developerPortfolio.FirstOrDefault(p => p.Visibility == Visibility.Public)
                    ?? developerPortfolio.FirstOrDefault();
                var displayName = $"{developer.User?.FristName} {developer.User?.LastName}".Trim();

                candidates.Add(new SuggestedCandidateDto
                {
                    CandidateType = "Developer",
                    Id = developer.UserId,
                    Name = string.IsNullOrWhiteSpace(displayName) ? developer.UserId.ToString() : displayName,
                    About = developer.Bio,
                    AvatarUrl = developer.ProfileImage,
                    CoverImageUrl = featured?.ImageCover,
                    AverageRating = developer.AverageRating,
                    RatingCount = developer.RatingCount,
                    Skills = developer.UserSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
                    Specialties = developer.UserSpecialties
                        .Select(s => string.IsNullOrWhiteSpace(s.Specialty.NameEn) ? s.Specialty.NameAr : s.Specialty.NameEn)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToList()!,
                    Categories = [],
                    PortfolioProjectCount = developerPortfolio.Count,
                    CompletedProjectsCount = developerPortfolio.Count(p => p.CompletionDate.HasValue),
                    MemberCount = null,
                    FeaturedPortfolioProjectId = featured?.Id,
                    FeaturedPortfolioTitle = featured?.Title,
                    FinalScore = item.FinalScore,
                    VectorScore = item.VectorScore,
                    Breakdown = MapBreakdown(item.Breakdown)
                });
            }
        }

        return candidates;
    }

    private async Task<DeveloperSuggestionDocument> MapDeveloperAsync(DeveloperProfile profile, CancellationToken ct)
    {
        var portfolios = await _portfolios.GetDeveloperPortfolioAsync(profile.UserId, ct);

        return new DeveloperSuggestionDocument
        {
            UserId = profile.UserId,
            Bio = profile.Bio,
            AverageRating = profile.AverageRating,
            RatingCount = profile.RatingCount,
            SkillNames = profile.UserSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
            SkillIds = profile.UserSkills.Select(s => s.SkillId).Distinct().ToList(),
            SpecialtyNames = profile.UserSpecialties
                .Select(s => string.IsNullOrWhiteSpace(s.Specialty.NameEn) ? s.Specialty.NameAr : s.Specialty.NameEn)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList()!,
            SpecialtyIds = profile.UserSpecialties.Select(s => s.SpecialtyId).Distinct().ToList(),
            CategoryNames = profile.UserInterests
                .Select(i => string.IsNullOrWhiteSpace(i.Category.NameEn) ? i.Category.Name : i.Category.NameEn)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList()!,
            CategoryIds = profile.UserInterests.Select(i => i.CategoryId).Distinct().ToList(),
            Portfolios = portfolios.Select(MapPortfolio).ToList()
        };
    }

    private async Task<TeamSuggestionDocument> MapTeamAsync(Team team, CancellationToken ct)
    {
        var portfolios = await _portfolios.GetTeamPortfolioAsync(team.Id, ct);
        var hasOpenJobs = team.TeamJobs?.Any(j => j.Status == TeamJobStatus.open)
            ?? (await _teamJobs.GetOpenByTeamIdAsync(team.Id, ct)).Count > 0;

        return new TeamSuggestionDocument
        {
            TeamId = team.Id,
            Name = team.Name,
            AboutUs = team.AboutUs,
            AverageRating = team.AverageRating,
            RatingCount = team.RatingCount,
            HasOpenJobs = hasOpenJobs,
            SkillNames = team.TeamSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
            SkillIds = team.TeamSkills.Select(s => s.SkillId).Distinct().ToList(),
            SpecialtyNames = team.TeamSpecialties
                .Select(s => string.IsNullOrWhiteSpace(s.Specialty.NameEn) ? s.Specialty.NameAr : s.Specialty.NameEn)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList()!,
            SpecialtyIds = team.TeamSpecialties.Select(s => s.SpecialtyId).Distinct().ToList(),
            CategoryNames = team.TeamCategories
                .Select(c => string.IsNullOrWhiteSpace(c.Category.NameEn) ? c.Category.Name : c.Category.NameEn)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList()!,
            CategoryIds = team.TeamCategories.Select(c => c.CategoryId).Distinct().ToList(),
            Portfolios = portfolios.Select(MapPortfolio).ToList()
        };
    }

    private async Task<TeamJobSuggestionDocument> MapTeamJobAsync(TeamJob job, CancellationToken ct)
    {
        var team = job.Team ?? await _teams.GetByIdWithDetailsAsync(job.TeamId, ct)
            ?? throw new InvalidOperationException($"Team {job.TeamId} not found for job {job.Id}.");

        var hasPortfolio = (await _portfolios.GetTeamPortfolioAsync(team.Id, ct)).Any();

        return new TeamJobSuggestionDocument
        {
            JobId = job.Id,
            TeamId = team.Id,
            Title = job.Title,
            Description = job.Description,
            Status = "open",
            TeamName = team.Name,
            TeamAbout = team.AboutUs,
            TeamAverageRating = team.AverageRating,
            TeamRatingCount = team.RatingCount,
            TeamHasPublicPortfolio = hasPortfolio,
            RequiredSkillNames = job.TeamJobSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
            RequiredSkillIds = job.TeamJobSkills.Select(s => s.SkillId).Distinct().ToList(),
            TeamSkillNames = team.TeamSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
        };
    }

    private static ProjectSuggestionDocument MapProject(Project project) => new()
    {
        ProjectId = project.Id,
        ClientId = project.ClientId,
        Title = project.Title,
        Description = project.Description,
        Status = "open",
        CategoryName = string.IsNullOrWhiteSpace(project.Category?.NameEn)
            ? project.Category?.Name
            : project.Category?.NameEn,
        CategoryId = project.CategoryId,
        BudgetMin = project.BudgetMin,
        BudgetMax = project.BudgetMax,
        Currency = string.IsNullOrWhiteSpace(project.Currency) ? "USD" : project.Currency,
        SkillNames = project.ProjectSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
        SkillIds = project.ProjectSkills.Select(s => s.SkillId).Distinct().ToList(),
        SpecialtyNames = project.ProjectSpecialties
            .Select(s => string.IsNullOrWhiteSpace(s.Specialty.NameEn) ? s.Specialty.NameAr : s.Specialty.NameEn)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList()!,
        SpecialtyIds = project.ProjectSpecialties.Select(s => s.SpecialtyId).Distinct().ToList()
    };

    private static PortfolioSnippet MapPortfolio(PortfolioProject portfolio) => new()
    {
        Title = portfolio.Title,
        Description = portfolio.Description,
        SkillNames = portfolio.PortfolioSkills?
            .Select(s => s.Skill?.Name ?? string.Empty)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList() ?? []
    };

    private static List<Guid> ParseDistinctGuids(IEnumerable<string> ids)
        => ids
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

    private static SuggestionScoreBreakdownDto MapBreakdown(ScoreBreakdown breakdown) => new()
    {
        Vector = breakdown.Vector,
        SkillOverlap = breakdown.SkillOverlap,
        SpecialtyOverlap = breakdown.SpecialtyOverlap,
        Rating = breakdown.Rating,
        Portfolio = breakdown.Portfolio
    };

    private static string GetMeta(IDictionary<string, string>? metadata, string key)
    {
        if (metadata is null)
            return string.Empty;

        return metadata.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static string DescribeSuggestionFailure(Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        if (message.Contains("Gemini", StringComparison.OrdinalIgnoreCase)
            || message.Contains("GEMINI_API_KEY", StringComparison.OrdinalIgnoreCase)
            || message.Contains("ApiKey", StringComparison.OrdinalIgnoreCase))
        {
            return "AI embeddings are not configured. Set GEMINI_API_KEY (or AI:GeminiApiKey), restart the API, then run suggestions reindex.";
        }

        if (message.Contains("Qdrant", StringComparison.OrdinalIgnoreCase)
            || message.Contains("vector", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Unavailable", StringComparison.OrdinalIgnoreCase))
        {
            return "Vector search is unavailable. Check the Qdrant connection, then reindex suggestions.";
        }

        return $"Could not load suggestions: {message}";
    }
}
