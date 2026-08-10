using System.Diagnostics;
using FreeGency.AI.Suggestions;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Suggestions.DTOs;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreeGency.Application.Features.Suggestions;

public sealed class SuggestionService : ISuggestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly SuggestionDocumentBuilder _documentBuilder;
    private readonly ISuggestionIndexingService _indexing;
    private readonly ISuggestionSearchService _search;
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
        ILogger<SuggestionService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _documentBuilder = documentBuilder;
        _indexing = indexing;
        _search = search;
        _logger = logger;

        _developers = _unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        _teams = _unitOfWork.Repository<ITeamRepository, Team>();
        _teamMembers = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _teamJobs = _unitOfWork.Repository<ITeamJobRepository, TeamJob>();
        _projects = _unitOfWork.Repository<IProjectRepository, Project>();
        _portfolios = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();
    }

    public async Task<ApiResponse<TeamsForMeResponseDto>> SuggestTeamsForMeAsync(int topK = 10, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var profile = await _developers.GetByUserIdWithSkillsAndInterestsAsync(_currentUser.UserId, ct);
            if (profile is null)
                return ApiResponse.Failure<TeamsForMeResponseDto>(AppError.NotFound(nameof(DeveloperProfile), _currentUser.UserId));

            var doc = await MapDeveloperAsync(profile, ct);
            var scored = await _search.SearchTeamJobsForDeveloperAsync(doc, topK, ct);

            var suggestions = new List<SuggestedTeamJobDto>();
            foreach (var item in scored)
            {
                if (!Guid.TryParse(item.Id, out var jobId))
                    continue;

                var job = await _teamJobs.GetByIdWithDetailsAsync(jobId, ct);
                if (job is null || job.Status != TeamJobStatus.open)
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
                    RequiredSkills = job.TeamJobSkills.Select(s => s.Skill.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
                    FinalScore = item.FinalScore,
                    VectorScore = item.VectorScore,
                    Breakdown = MapBreakdown(item.Breakdown)
                });
            }

            sw.Stop();
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
        try
        {
        var project = await _projects.GetByIdWithDetailsAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(AppError.NotFound(nameof(Project), projectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(AppError.Forbidden("You do not own this project."));

        if (project.Status != ProjectStatus.Open)
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                AppError.Validation("Project must be published (Open) before requesting candidate suggestions."));

        var projectDoc = MapProject(project);
        var scored = await _search.SearchCandidatesForProjectAsync(projectDoc, topK, ct);

        var candidates = new List<SuggestedCandidateDto>();
        foreach (var item in scored)
        {
            if (!Guid.TryParse(item.Id, out var id))
                continue;

            if (string.Equals(item.CandidateType, "Team", StringComparison.OrdinalIgnoreCase))
            {
                var team = await _teams.GetByIdWithDetailsAsync(id, ct);
                if (team is null)
                    continue;

                var teamPortfolio = await _portfolios.Query()
                    .AsNoTracking()
                    .Where(p => p.OwnerTeamId == team.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(ct);
                var featured = teamPortfolio.FirstOrDefault(p => p.Visibility == Visibility.Public)
                    ?? teamPortfolio.FirstOrDefault();
                var completedCount = await _projects.GetProjectsQuery()
                    .AsNoTracking()
                    .CountAsync(p => p.AssignedTeamId == team.Id && p.Status == ProjectStatus.Completed, ct);
                var memberCount = await _teamMembers.GetMemberCountAsync(team.Id, ct);

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
                        .ToList(),
                    Categories = team.TeamCategories
                        .Select(c => string.IsNullOrWhiteSpace(c.Category.NameEn) ? c.Category.Name : c.Category.NameEn)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .ToList(),
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
                var developer = await _developers.Query()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(d => d.User)
                    .Include(d => d.UserSkills).ThenInclude(s => s.Skill)
                    .Include(d => d.UserSpecialties).ThenInclude(s => s.Specialty)
                    .FirstOrDefaultAsync(d => d.UserId == id, ct);

                if (developer is null)
                    continue;

                var developerPortfolio = await _portfolios.Query()
                    .AsNoTracking()
                    .Where(p => p.OwnerUserId == developer.UserId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(ct);
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
                        .ToList(),
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
            _logger.LogError(ex, "SuggestCandidatesForProject failed for project {ProjectId}", projectId);
            return ApiResponse.Failure<ProjectCandidatesResponseDto>(
                AppError.Validation(DescribeSuggestionFailure(ex)));
        }
    }

    public async Task<ApiResponse<ReindexResultDto>> ReindexAllAsync(CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting full suggestions reindex.");

        await _indexing.ResetCollectionsAsync(ct);

        var developerDocs = new List<BuiltSuggestionDocument>();
        var developers = await _developers.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(d => d.UserSkills).ThenInclude(s => s.Skill)
            .Include(d => d.UserSpecialties).ThenInclude(s => s.Specialty)
            .Include(d => d.UserInterests).ThenInclude(i => i.Category)
            .ToListAsync(ct);

        foreach (var developer in developers)
        {
            var mapped = await MapDeveloperAsync(developer, ct);
            developerDocs.Add(_documentBuilder.BuildDeveloper(mapped));
        }

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
        {
            var mapped = await MapTeamAsync(team, ct);
            teamDocs.Add(_documentBuilder.BuildTeam(mapped));
        }

        var jobDocs = new List<BuiltSuggestionDocument>();
        var openJobs = await _teamJobs.Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(j => j.Status == TeamJobStatus.open)
            .Include(j => j.Team).ThenInclude(t => t.TeamSkills).ThenInclude(s => s.Skill)
            .Include(j => j.TeamJobSkills).ThenInclude(s => s.Skill)
            .ToListAsync(ct);

        foreach (var job in openJobs)
        {
            var mapped = await MapTeamJobAsync(job, ct);
            jobDocs.Add(_documentBuilder.BuildTeamJob(mapped));
        }

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

        await _indexing.UpsertBatchAsync(developerDocs, ct);
        await _indexing.UpsertBatchAsync(teamDocs, ct);
        await _indexing.UpsertBatchAsync(jobDocs, ct);
        await _indexing.UpsertBatchAsync(projectDocs, ct);

        sw.Stop();
        _logger.LogInformation(
            "Suggestions reindex finished in {Elapsed}ms (devs={Devs}, teams={Teams}, jobs={Jobs}, projects={Projects}).",
            sw.ElapsedMilliseconds, developerDocs.Count, teamDocs.Count, jobDocs.Count, projectDocs.Count);

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

        var doc = await MapDeveloperAsync(profile, ct);
        await _indexing.UpsertAsync(_documentBuilder.BuildDeveloper(doc), ct);
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

        var doc = await MapTeamAsync(team, ct);
        await _indexing.UpsertAsync(_documentBuilder.BuildTeam(doc), ct);
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

        var doc = await MapTeamJobAsync(job, ct);
        await _indexing.UpsertAsync(_documentBuilder.BuildTeamJob(doc), ct);
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
    {
        await _indexing.DeleteAsync(SuggestionCollections.Projects, projectId.ToString(), ct);
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

    private static ProjectSuggestionDocument MapProject(Project project)
    {
        return new ProjectSuggestionDocument
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
    }

    private static PortfolioSnippet MapPortfolio(PortfolioProject portfolio)
    {
        return new PortfolioSnippet
        {
            Title = portfolio.Title,
            Description = portfolio.Description,
            SkillNames = portfolio.PortfolioSkills?
                .Select(s => s.Skill?.Name ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList() ?? []
        };
    }

    private static SuggestionScoreBreakdownDto MapBreakdown(ScoreBreakdown breakdown)
        => new()
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
