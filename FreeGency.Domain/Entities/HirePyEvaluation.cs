using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

/// <summary>
/// Structured AI evaluation of one freelancer candidate for a HirePy session, produced after the
/// private technical discussion and the milestone plan are finalized. Used to pick the single
/// recommended candidate — the decision weighs the actual interaction and the milestone plan,
/// not just the original ranking.
/// </summary>
public class HirePyEvaluation : ISoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid HirePySessionId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>The proposal this evaluation belongs to.</summary>
    public Guid ProjectProposalId { get; set; }

    /// <summary>The freelancer user (solo dev, or the accepting team leader).</summary>
    public Guid FreelancerUserId { get; set; }

    public string CandidateName { get; set; } = string.Empty;

    /// <summary>The candidate's original ranking position (1-based) from the ranking phase.</summary>
    public int RankingPosition { get; set; }

    /// <summary>The candidate's original ranking score (0-100). Re-ranked at evaluation time, with a
    /// rank-derived fallback when the ranking service is unavailable.</summary>
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

    /// <summary>JSON arrays. Never include raw private conversation content — only client-safe summaries.</summary>
    public string? StrengthsJson { get; set; }
    public string? ConcernsJson { get; set; }
    public string? RisksJson { get; set; }

    public string? Reason { get; set; }

    /// <summary>Summary of the candidate's finalized milestone plan (titles, costs, durations).</summary>
    public string? MilestoneSummary { get; set; }

    public decimal? ProposedBudget { get; set; }
    public string? ProposedTimeline { get; set; }

    public DateTime? EvaluatedAt { get; set; }
}
