using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class Specialty : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;

    public virtual ICollection<Category> Categories { get; set; } = [];
    public virtual ICollection<SpecialtySkill> SpecialtySkills { get; set; } = [];
    public virtual ICollection<TeamSpecialty> TeamSpecialties { get; set; } = [];
    public virtual ICollection<ProjectSpecialty> ProjectSpecialties { get; set; } = [];
}
