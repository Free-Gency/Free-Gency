
namespace FreeGency.Domain.Entities.Plans;

public class Plan: ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public string Name { get; set; } = "Free";
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<PlanFeature> Features { get; set; }
        = new List<PlanFeature>();
}
