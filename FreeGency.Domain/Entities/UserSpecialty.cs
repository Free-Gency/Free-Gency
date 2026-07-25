namespace FreeGency.Domain.Entities;

public class UserSpecialty : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid? ClientProfileId { get; set; }
    public Guid? DeveloperProfileId { get; set; }
    public Guid SpecialtyId { get; set; }

    public virtual ClientProfile? ClientProfile { get; set; }
    public virtual DeveloperProfile? DeveloperProfile { get; set; }
    public virtual Specialty Specialty { get; set; } = null!;
}
