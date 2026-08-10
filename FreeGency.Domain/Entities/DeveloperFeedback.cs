using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class DeveloperFeedback : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid DeveloperUserId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Visible;
    public string? ModerationNote { get; set; }
    public string? ModeratedText { get; set; }

    public virtual User DeveloperUser { get; set; } = null!;
    public virtual User ReviewerUser { get; set; } = null!;
}
