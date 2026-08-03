

namespace FreeGency.Application.Features.TeamSuggestions;

public sealed class TeamSuggestionService : ITeamSuggestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly TeamSuggestionChatService _ai;

    private readonly IUserRepository _userRepository;
    private readonly ITeamJobRepository _teamJobRepository;
    private readonly ITeamJoinRequestRepository _teamJoinRequestRepository;

    public TeamSuggestionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, TeamSuggestionChatService ai)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _ai = ai;
        _userRepository = _unitOfWork.Repository<IUserRepository, User>();
        _teamJobRepository = _unitOfWork.Repository<ITeamJobRepository, TeamJob>();
        _teamJoinRequestRepository = _unitOfWork.Repository<ITeamJoinRequestRepository, TeamJoinRequest>();
    }

    public async Task<ApiResponse<TeamSuggestionResponseDto>> SuggestTeamsForDeveloperAsync(
        int topK = 10,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var developerId = _currentUser.UserId;

        var user = await _userRepository.Query()
            .Where(u => u.Id == developerId)
            .Include(u => u.DeveloperProfile!).ThenInclude(dp => dp.UserSkills).ThenInclude(us => us.Skill)
            .Include(u => u.DeveloperProfile!).ThenInclude(dp => dp.UserSpecialties).ThenInclude(us => us.Specialty).ThenInclude(s => s.CategorySpecialties).ThenInclude(cs => cs.Category)
            .Include(u => u.DeveloperProfile!).ThenInclude(dp => dp.UserInterests).ThenInclude(ui => ui.Category)
            .FirstOrDefaultAsync(ct);

        if (user?.DeveloperProfile is null)
            return ApiResponse.Failure<TeamSuggestionResponseDto>(
                AppError.Validation("A developer profile is required to get team suggestions."));

        var dev = user.DeveloperProfile;
        var devSkillIds = dev.UserSkills.Select(s => s.SkillId).ToHashSet();
        var devSpecialtyIds = dev.UserSpecialties.Select(s => s.SpecialtyId).ToHashSet();
        var devCategoryIds = dev.UserInterests.Select(i => i.CategoryId)
            .Concat(dev.UserSpecialties.SelectMany(s => s.Specialty.CategorySpecialties.Select(cs => cs.CategoryId)))
            .ToHashSet();

        // 1) Load OPEN team jobs (with team + job skills). Only teams with an open job can be suggested.
        var openJobs = await _teamJobRepository.Query()
            .AsNoTracking()
            .Where(j => j.Status == TeamJobStatus.open && !j.IsDeleted)
            .Include(j => j.Team).ThenInclude(t => t.TeamMembers)
            .Include(j => j.Team).ThenInclude(t => t.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(j => j.Team).ThenInclude(t => t.TeamSpecialties).ThenInclude(ts => ts.Specialty)
            .Include(j => j.Team).ThenInclude(t => t.TeamCategories).ThenInclude(tc => tc.Category)
            .Include(j => j.TeamJobSkills).ThenInclude(js => js.Skill)
            .Take(200)
            .ToListAsync(ct);

        if (openJobs.Count == 0)
        {
            return ApiResponse.Success(new TeamSuggestionResponseDto
            {
                DeveloperId = developerId.ToString(),
                Metadata = new TeamSuggestionMetadataDto
                {
                    TotalCandidatesEvaluated = 0,
                    ReturnedCount = 0,
                    ProcessingTimeMs = sw.Elapsed.TotalMilliseconds,
                    Warnings = ["No teams with open jobs right now."]
                }
            });
        }

        // 2) Rule-match: keep teams whose open job shares the developer's skills / specialties / categories.
        //    Per team, keep its BEST matching job.
        var appliedTeamIds = (await _teamJoinRequestRepository.Query()
            .AsNoTracking()
            .Where(jr => jr.UserId == developerId)
            .Select(jr => jr.TeamId)
            .ToListAsync(ct)).ToHashSet();

        var matched = MatchCandidates(openJobs, devSkillIds, devSpecialtyIds, devCategoryIds);

        if (matched.Count == 0)
        {
            return ApiResponse.Success(new TeamSuggestionResponseDto
            {
                DeveloperId = developerId.ToString(),
                Metadata = new TeamSuggestionMetadataDto
                {
                    TotalCandidatesEvaluated = openJobs.Count,
                    ReturnedCount = 0,
                    ProcessingTimeMs = sw.Elapsed.TotalMilliseconds,
                    Warnings = ["No open team jobs match your skills yet. Try adding more skills to your profile."]
                }
            });
        }

        // 3) Pre-select top candidates for the AI prompt, then ask AI to rank.
        var preSelected = matched
            .OrderByDescending(m => m.RuleScore)
            .Take(15)
            .ToList();

        var request = MapToRequest(user, dev, preSelected);
        var aiResult = await _ai.SuggestAsync(request, ct);
        sw.Stop();

        var ranked = MapRanked(preSelected, aiResult, appliedTeamIds, topK, out var usedFallback);

        return ApiResponse.Success(new TeamSuggestionResponseDto
        {
            DeveloperId = developerId.ToString(),
            AiSummary = aiResult.OverallSummary,
            RankedTeams = ranked,
            Metadata = new TeamSuggestionMetadataDto
            {
                TotalCandidatesEvaluated = matched.Count,
                ReturnedCount = ranked.Count,
                ProcessingTimeMs = sw.Elapsed.TotalMilliseconds,
                UsedFallbackScoring = usedFallback,
                Warnings = usedFallback ? ["AI ranking unavailable — used skill-match scoring."] : []
            }
        });
    }

    // --- Rule-based matching + scoring ---

    private static List<TeamCandidateMatch> MatchCandidates(
        List<TeamJob> openJobs,
        HashSet<Guid> devSkillIds,
        HashSet<Guid> devSpecialtyIds,
        HashSet<Guid> devCategoryIds)
    {
        return openJobs
            .GroupBy(j => j.TeamId)
            .SelectMany(group =>
            {
                var team = group.First().Team;
                var teamSpecialtyIds = team.TeamSpecialties.Select(ts => ts.SpecialtyId).ToHashSet();
                var teamCategoryIds = team.TeamCategories.Select(tc => tc.CategoryId).ToHashSet();

                // best job = max skill overlap
                var bestJob = group
                    .OrderByDescending(j =>
                    {
                        var jobSkillIds = j.TeamJobSkills.Select(js => js.SkillId).ToHashSet();
                        return jobSkillIds.Count(devSkillIds.Contains);
                    })
                    .First();

                var jobSkillIds = bestJob.TeamJobSkills.Select(js => js.SkillId).ToHashSet();
                var skillOverlap = jobSkillIds.Count(devSkillIds.Contains);
                var specialtyOverlap = teamSpecialtyIds.Count(devSpecialtyIds.Contains);
                var categoryOverlap = teamCategoryIds.Count(devCategoryIds.Contains);

                var matches = skillOverlap + specialtyOverlap + categoryOverlap;
                if (matches == 0)
                    return Enumerable.Empty<TeamCandidateMatch>();

                var score = skillOverlap * 0.6
                            + specialtyOverlap * 0.25
                            + categoryOverlap * 0.15
                            + (double)team.AverageRating / 5d * 0.2;

                return new[]
                {
                    new TeamCandidateMatch
                    {
                        Team = team,
                        Job = bestJob,
                        JobSkillIds = jobSkillIds,
                        SkillOverlap = skillOverlap,
                        SpecialtyOverlap = specialtyOverlap,
                        CategoryOverlap = categoryOverlap,
                        RuleScore = score
                    }
                };
            })
            .OrderByDescending(m => m.RuleScore)
            .ToList();
    }

    private static TeamSuggestionRequest MapToRequest(User user, DeveloperProfile dev, List<TeamCandidateMatch> matches)
    {
        return new TeamSuggestionRequest
        {
            DeveloperName = $"{user.FristName} {user.LastName}",
            DeveloperSkills = dev.UserSkills.Select(s => s.Skill.Name).ToList(),
            DeveloperSpecialties = dev.UserSpecialties.Select(s => s.Specialty.NameEn).ToList(),
            DeveloperCategories = dev.UserInterests.Select(i => i.Category.NameEn)
                .Concat(dev.UserSpecialties.SelectMany(s => s.Specialty.CategorySpecialties.Select(cs => cs.Category.NameEn)))
                .Distinct().ToList(),
            TopK = 10,
            Candidates = matches.Select(m => new TeamCandidateInput
            {
                TeamId = m.Team.Id.ToString(),
                Name = m.Team.Name,
                Skills = m.Team.TeamSkills.Select(ts => ts.Skill.Name).ToList(),
                Specialties = m.Team.TeamSpecialties.Select(ts => ts.Specialty.NameEn).ToList(),
                Categories = m.Team.TeamCategories.Select(tc => tc.Category.NameEn).ToList(),
                MemberCount = m.Team.TeamMembers.Count,
                AverageRating = (double)m.Team.AverageRating,
                RatingCount = m.Team.RatingCount,
                OpenJobs = new List<OpenJobInput>
                {
                    new OpenJobInput
                    {
                        JobId = m.Job.Id.ToString(),
                        Title = m.Job.Title,
                        Description = m.Job.Description,
                        Skills = m.Job.TeamJobSkills.Select(js => js.Skill.Name).ToList()
                    }
                }
            }).ToList()
        };
    }

    private static List<RankedTeamSuggestionDto> MapRanked(
        List<TeamCandidateMatch> matches,
        TeamSuggestionAiResult aiResult,
        HashSet<Guid> appliedTeamIds,
        int topK,
        out bool usedFallback)
    {
        var matchByTeamId = matches.ToDictionary(m => m.Team.Id.ToString());
        var result = new List<RankedTeamSuggestionDto>();
        usedFallback = false;

        foreach (var ai in aiResult.Candidates.Take(topK))
        {
            if (!matchByTeamId.TryGetValue(ai.TeamId, out var match))
                continue;

            var jobId = ai.JobId ?? match.Job.Id.ToString();
            result.Add(new RankedTeamSuggestionDto
            {
                TeamId = ai.TeamId,
                TeamName = match.Team.Name,
                TeamLogoUrl = match.Team.Logo,
                JobId = jobId,
                JobTitle = match.Job.Title,
                Score = ai.Score,
                Confidence = ai.Confidence,
                Summary = ai.Summary,
                Reason = ai.Reason,
                Strengths = ai.Strengths,
                Weaknesses = ai.Weaknesses,
                JobSkills = match.Job.TeamJobSkills.Select(js => js.Skill.Name).ToList(),
                MemberCount = match.Team.TeamMembers.Count,
                AverageRating = (double)match.Team.AverageRating,
                RatingCount = match.Team.RatingCount,
                HasApplied = appliedTeamIds.Contains(match.Team.Id)
            });
        }

        if (result.Count > 0)
            return result;

        // Deterministic fallback: return rule scores, never empty when matches exist
        usedFallback = true;
        return matches
            .Take(topK)
            .Select(m => new RankedTeamSuggestionDto
            {
                TeamId = m.Team.Id.ToString(),
                TeamName = m.Team.Name,
                TeamLogoUrl = m.Team.Logo,
                JobId = m.Job.Id.ToString(),
                JobTitle = m.Job.Title,
                Score = Math.Clamp(m.RuleScore, 0d, 1d),
                Confidence = 0.5,
                Summary = $"{m.SkillOverlap} matching skills, {m.SpecialtyOverlap} matching specialties.",
                Strengths = m.Job.TeamJobSkills.Select(js => js.Skill.Name).Take(6).ToList(),
                Weaknesses = [],
                JobSkills = m.Job.TeamJobSkills.Select(js => js.Skill.Name).ToList(),
                MemberCount = m.Team.TeamMembers.Count,
                AverageRating = (double)m.Team.AverageRating,
                RatingCount = m.Team.RatingCount,
                HasApplied = appliedTeamIds.Contains(m.Team.Id)
            })
            .ToList();
    }

    private sealed class TeamCandidateMatch
    {
        public Team Team { get; set; } = null!;
        public TeamJob Job { get; set; } = null!;
        public HashSet<Guid> JobSkillIds { get; set; } = [];
        public int SkillOverlap { get; set; }
        public int SpecialtyOverlap { get; set; }
        public int CategoryOverlap { get; set; }
        public double RuleScore { get; set; }
    }
}
