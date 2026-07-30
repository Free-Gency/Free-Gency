using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class MilestonePlanItem : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid PlanVersionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DefinitionOfDone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public int SortOrder { get; set; }
    /// <summary>New | Updated | null when unchanged vs previous version.</summary>
    public string? ChangeTag { get; set; }

    public virtual MilestonePlanVersion PlanVersion { get; set; } = null!;
}
