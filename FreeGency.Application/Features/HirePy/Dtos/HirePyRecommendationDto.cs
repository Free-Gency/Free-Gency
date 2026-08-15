namespace FreeGency.Application.Features.HirePy.Dtos;

/// <summary>
/// Client-safe final recommendation for a HirePy session. Contains only the client-visible project
/// details and the recommended candidate's summary. Never exposes system prompts, internal AI
/// reasoning, raw private conversation content, or other candidates' information.
/// </summary>
public sealed class HirePyRecommendationDto
{
    public Guid SessionId { get; set; }
    public Guid ProjectId { get; set; }
    public string Stage { get; set; } = string.Empty;
    public DateTime? RecommendationCompletedAt { get; set; }

    public HirePyRecommendationProjectDto Project { get; set; } = new();
    public HirePyRecommendationFreelancerDto Freelancer { get; set; } = new();
    public HirePyRecommendationProposalDto Proposal { get; set; } = new();
    public HirePyRecommendationEvaluationDto Evaluation { get; set; } = new();
}

public sealed class HirePyRecommendationProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = [];
    public List<string> Skills { get; set; } = [];
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsFixedPrice { get; set; }
    public DateTime? Deadline { get; set; }
    public int? EstimatedDurationDays { get; set; }
    public string? CategoryName { get; set; }
}

public sealed class HirePyRecommendationFreelancerDto
{
    public Guid FreelancerUserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int RankingPosition { get; set; }
    public int RankingScore { get; set; }
}

public sealed class HirePyRecommendationProposalDto
{
    public Guid ProposalId { get; set; }
    public string? CoverLetter { get; set; }
    public string? Approach { get; set; }
    public decimal? ProposedBudget { get; set; }
    public string? ProposedTimeline { get; set; }
}

/// <summary>AI evaluation summary of the recommended candidate (client-safe only).</summary>
public sealed class HirePyRecommendationEvaluationDto
{
    public int OverallScore { get; set; }
    public int TechnicalScore { get; set; }
    public int RequirementsScore { get; set; }
    public int ArchitectureScore { get; set; }
    public int ImplementationScore { get; set; }
    public int MilestoneScore { get; set; }
    public int TimelineScore { get; set; }
    public int BudgetScore { get; set; }
    public int CommunicationScore { get; set; }
    public int RiskScore { get; set; }

    public List<string> Strengths { get; set; } = [];
    public List<string> Concerns { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public string? Reason { get; set; }
    public string? MilestoneSummary { get; set; }
    public string? BudgetCompatibility { get; set; }
}
