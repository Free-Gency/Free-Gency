using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class TeamSpecialty : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid TeamId { get; set; }
    public Guid SpecialtyId { get; set; }

    public virtual Team Team { get; set; } = null!;
    public virtual Specialty Specialty { get; set; } = null!;
}
