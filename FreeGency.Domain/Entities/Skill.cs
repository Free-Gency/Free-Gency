using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class Skill : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public string Name { get; set; } = string.Empty;

    public virtual ICollection<UserSkill> UserSkills { get; set; } = [];
    public virtual ICollection<TeamSkill> TeamSkills { get; set; } = [];
    public virtual ICollection<TeamJobSkill> TeamJobSkills { get; set; } = [];
    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = [];
    public virtual ICollection<PortfolioSkill> PortfolioSkills { get; set; } = [];
}