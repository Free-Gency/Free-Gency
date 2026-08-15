namespace FreeGency.Application.Features.HirePy.Dtos;

public sealed class HirePySessionDto
{
    public Guid Id { get; set; }
    public Guid ClientUserId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
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
    public List<Guid> SkillIds { get; set; } = [];
    public List<Guid> SpecialtyIds { get; set; } = [];
    public List<string> Requirements { get; set; } = [];
    public List<string> Features { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public List<HirePySelectedCandidateDto> SelectedCandidates { get; set; } = [];

    public Guid? SelectedProposalId { get; set; }
    public Guid? SelectedFreelancerUserId { get; set; }
    public string? SelectedCandidateName { get; set; }
    public string? DecisionReason { get; set; }
    public string? MilestoneSummary { get; set; }
    public decimal? SelectedBudget { get; set; }
    public string? SelectedTimeline { get; set; }
    public DateTime? RecommendationCompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AcceptedPlanVersionId { get; set; }
}
