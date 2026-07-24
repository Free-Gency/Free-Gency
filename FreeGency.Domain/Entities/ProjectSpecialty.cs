namespace FreeGency.Domain.Entities;

public class ProjectSpecialty : ISoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ProjectId { get; set; }
    public Guid SpecialtyId { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual Specialty Specialty { get; set; } = null!;
}
