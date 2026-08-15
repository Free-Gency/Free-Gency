namespace FreeGency.Domain.Entities;

public class HirePySession : ISoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ClientUserId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Description { get; set; } = string.Empty;
    public HirePySessionStatus Status { get; set; } = HirePySessionStatus.Draft;
    public string? FailReason { get; set; }

    public string? Title { get; set; }
    public string? GeneratedDescription { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsFixedPrice { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? Currency { get; set; }
    public DateTime? Deadline { get; set; }
    public int? EstimatedDurationDays { get; set; }
    public string? Complexity { get; set; }

    public string? SkillIdsJson { get; set; }
    public string? SpecialtyIdsJson { get; set; }
    public string? RequirementsJson { get; set; }
    public string? FeaturesJson { get; set; }
    public string? RisksJson { get; set; }

    /// <summary>JSON list of the selected top candidates chosen after ranking.</summary>
    public string? SelectedCandidatesJson { get; set; }

    /// <summary>The single candidate selected by the evaluation phase. Not necessarily the top original rank.</summary>
    public Guid? SelectedProposalId { get; set; }
    public Guid? SelectedFreelancerUserId { get; set; }
    public string? SelectedCandidateName { get; set; }
    public string? DecisionReason { get; set; }
    public string? MilestoneSummary { get; set; }
    public decimal? SelectedBudget { get; set; }
    public string? SelectedTimeline { get; set; }
    public DateTime? RecommendationCompletedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>The agreed milestone plan version accepted by the existing hiring service.</summary>
    public Guid? AcceptedPlanVersionId { get; set; }

    public virtual Project? Project { get; set; }
}
