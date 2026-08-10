
namespace FreeGency.Domain.Entities;

public class MilestoneAssignment : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid MilestoneId { get; set; }
    public Guid UserId { get; set; }


    public decimal Percentage { get; set; }

    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }

    public virtual Milestone Milestone { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}