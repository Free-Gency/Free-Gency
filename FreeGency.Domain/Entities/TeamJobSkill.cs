using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class TeamJobSkill : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid TeamJobId { get; set; }
    public Guid SkillId { get; set; }

    public virtual TeamJob TeamJob { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
