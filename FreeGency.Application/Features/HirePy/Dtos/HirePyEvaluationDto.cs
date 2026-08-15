namespace FreeGency.Application.Features.HirePy.Dtos;

/// <summary>
/// Client-safe structured evaluation of one candidate. Mirrors the spec JSON:
/// scores 0-100 plus strengths/concerns/risks/reason (architectureScore and implementationScore
/// are an additive superset of the required keys).
/// </summary>
public sealed class HirePyEvaluationDto
{
    public Guid ProposalId { get; set; }
    public Guid FreelancerUserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int RankingPosition { get; set; }
    public int RankingScore { get; set; }

    public int TechnicalScore { get; set; }
    public int RequirementsScore { get; set; }
    public int ArchitectureScore { get; set; }
    public int ImplementationScore { get; set; }
    public int MilestoneScore { get; set; }
    public int TimelineScore { get; set; }
    public int BudgetScore { get; set; }
    public int CommunicationScore { get; set; }
    public int RiskScore { get; set; }
    public int OverallScore { get; set; }

    public List<string> Strengths { get; set; } = [];
    public List<string> Concerns { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public string? Reason { get; set; }

    public string? MilestoneSummary { get; set; }
    public decimal? ProposedBudget { get; set; }
    public string? ProposedTimeline { get; set; }
    public DateTime? EvaluatedAt { get; set; }
}
