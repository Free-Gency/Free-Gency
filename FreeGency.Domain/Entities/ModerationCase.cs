using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class ModerationCase : IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid UserId { get; set; }
    public ModerationSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public string ContentSnapshot { get; set; } = string.Empty;
    public string Categories { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public ModerationAction Action { get; set; }
    public ModerationCaseStatus Status { get; set; } = ModerationCaseStatus.AutoResolved;
    public string? UserMessage { get; set; }
    public string? AdminSummary { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }

    public virtual User User { get; set; } = null!;
}
