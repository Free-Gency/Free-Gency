using FreeGency.Domain.Constants;

namespace FreeGency.AI.ProjectDrafting;

public class GeneratedProjectDraft
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool NeedsManualCategoryReview { get; set; }
    public List<Guid> SpecialtyIds { get; set; } = [];
    public List<string> SpecialtyNames { get; set; } = [];
    public List<Guid> SkillIds { get; set; } = [];
    public List<string> SkillNames { get; set; } = [];

    public bool IsFixedPrice { get; set; } = true;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public string Currency { get; set; } = Currencies.USD;
    public int? EstimatedDurationDays { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Complexity { get; set; }
    public List<string> Requirements { get; set; } = [];
    public List<string> Features { get; set; } = [];
    public List<string> Risks { get; set; } = [];
}
