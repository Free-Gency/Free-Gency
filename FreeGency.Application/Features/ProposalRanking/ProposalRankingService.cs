using FreeGency.AI.Interfaces;
using FreeGency.AI.Ranking.ProposalRanking;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using Microsoft.EntityFrameworkCore;

using IApplicationProposalRankingService = FreeGency.Application.Common.Interfaces.IProposalRankingService;
using IOrchestrator = FreeGency.AI.Interfaces.IAIOrchestrator;

namespace FreeGency.Application.Features.ProposalRanking;

public sealed class ProposalRankingService : IApplicationProposalRankingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrchestrator _orchestrator;

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly ITeamRepository _teamRepository;

    public ProposalRankingService(IUnitOfWork unitOfWork, IOrchestrator orchestrator)
    {
        _unitOfWork = unitOfWork;
        _orchestrator = orchestrator;
        _projectRepository = _unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = _unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _teamRepository = _unitOfWork.Repository<ITeamRepository, Team>();
    }

    public async Task<ApiResponse<ProjectRankingResponse>> RankAsync(Guid projectId, int topK = 10, CancellationToken ct = default)
    {
        var project = await _projectRepository.Query()
            .Where(p => p.Id == projectId)
            .Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return ApiResponse.Failure<ProjectRankingResponse>(AppError.NotFound(nameof(Project), projectId));

        // Rank open + decided proposals so clients still see match scores — skip withdrawn only.
        var proposals = await _proposalRepository.Query()
            .Where(p => p.ProjectId == projectId && p.Status != ProposalStatus.Withdrawn)
            .Include(p => p.Team).ThenInclude(t => t!.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(p => p.User).ThenInclude(u => u!.DeveloperProfile).ThenInclude(dp => dp!.UserSkills).ThenInclude(us => us.Skill)
            .ToListAsync(ct);

        if (proposals.Count == 0)
        {
            return ApiResponse.Success(new ProjectRankingResponse
            {
                ProjectId = project.Id.ToString(),
                RankedProposals = [],
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = 0,
                    ReturnedCount = 0,
                    ProcessingTime = TimeSpan.Zero,
                    Warnings = ["No proposals found for this project."]
                }
            });
        }

        var request = MapToRequest(project, proposals, topK);
        var result = await _orchestrator.RankProjectAsync(request, ct);
        return ApiResponse.Success(result);
    }

    private static ProjectRankingRequest MapToRequest(Project project, List<ProjectProposal> proposals, int topK)
    {
        var requiredSkills = project.ProjectSkills
            .Select(ps => ps.Skill.Name)
            .ToList();

        var candidates = proposals.Select(MapToCandidate).ToList();

        return new ProjectRankingRequest
        {
            ProjectId = project.Id.ToString(),
            Title = project.Title,
            Description = project.Description,
            Criteria = new RankingCriteria
            {
                RequiredSkills = requiredSkills,
                BudgetMin = project.BudgetMin,
                BudgetMax = project.BudgetMax,
                Timeline = project.EstimatedDurationDays.HasValue
                    ? $"{project.EstimatedDurationDays.Value} days"
                    : null
            },
            Candidates = candidates,
            TopK = topK
        };
    }

    private static ProposalCandidate MapToCandidate(ProjectProposal proposal)
    {
        return proposal.ApplicantType switch
        {
            ApplicantType.Team => MapTeamCandidate(proposal),
            ApplicantType.User => MapUserCandidate(proposal),
            _ => MapFallbackCandidate(proposal)
        };
    }

    private static ProposalCandidate MapTeamCandidate(ProjectProposal proposal)
    {
        var team = proposal.Team!;
        var skills = team.TeamSkills?
            .Select(ts => new CandidateSkill
            {
                Name = ts.Skill.Name,
                Proficiency = SkillProficiency.Intermediate
            })
            .ToList() ?? [];

        return new ProposalCandidate
        {
            Id = proposal.Id.ToString(),
            Name = team.Name,
            Headline = team.AboutUs,
            Bio = team.AboutUs,
            CoverLetter = proposal.CoverLetter,
            Skills = skills,
            Pricing = new CandidatePricing
            {
                FixedPriceEstimate = proposal.ProposedBudget,
                Currency = "USD"
            },
            Reputation = new CandidateReputation
            {
                AverageRating = (double?)team.AverageRating,
                TotalReviews = team.RatingCount,
                IsVerified = true
            },
            MatchContext = $"Team proposal with {skills.Count} skills"
        };
    }

    private static ProposalCandidate MapUserCandidate(ProjectProposal proposal)
    {
        var user = proposal.User!;
        var devProfile = user.DeveloperProfile;
        var skills = devProfile?.UserSkills?
            .Select(us => new CandidateSkill
            {
                Name = us.Skill.Name,
                Proficiency = SkillProficiency.Intermediate
            })
            .ToList() ?? [];

        return new ProposalCandidate
        {
            Id = proposal.Id.ToString(),
            Name = $"{user.FristName} {user.LastName}",
            Headline = devProfile?.Bio,
            Bio = devProfile?.Bio,
            CoverLetter = proposal.CoverLetter,
            Skills = skills,
            Pricing = new CandidatePricing
            {
                FixedPriceEstimate = proposal.ProposedBudget,
                Currency = "USD"
            },
            Reputation = new CandidateReputation
            {
                AverageRating = devProfile is not null ? (double)devProfile.AverageRating : null,
                TotalReviews = devProfile?.RatingCount,
                IsVerified = user.IsVerified
            },
            MatchContext = $"Individual proposal with {skills.Count} skills"
        };
    }

    private static ProposalCandidate MapFallbackCandidate(ProjectProposal proposal)
    {
        return new ProposalCandidate
        {
            Id = proposal.Id.ToString(),
            Name = "Unknown Applicant",
            CoverLetter = proposal.CoverLetter,
            Skills = [],
            Pricing = new CandidatePricing
            {
                FixedPriceEstimate = proposal.ProposedBudget,
                Currency = "USD"
            }
        };
    }
}
