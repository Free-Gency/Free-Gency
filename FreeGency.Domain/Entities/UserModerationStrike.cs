using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class UserModerationStrike : IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid UserId { get; set; }
    public Guid? ModerationCaseId { get; set; }
    public ModerationCategory PrimaryCategory { get; set; }
    public string Reason { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
    public virtual ModerationCase? ModerationCase { get; set; }
}
