using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class Message : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ChatRoomId { get; set; }
    public Guid? SenderClientProfileId { get; set; }
    public Guid? SenderDeveloperProfileId { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string? Text { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public Guid? PlanVersionId { get; set; }
    public Guid? MilestoneId { get; set; }
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Visible;
    public string? ModerationNote { get; set; }
    /// <summary>Public-facing text when status is Hidden/Redacted; null means use Text.</summary>
    public string? ModeratedText { get; set; }

    /// <summary>True when the Hiring Agent posted this message on behalf of the client.</summary>
    public bool IsAgentGenerated { get; set; }

    public virtual ChatRoom ChatRoom { get; set; } = null!;
    public virtual ClientProfile? SenderClientProfile { get; set; }
    public virtual DeveloperProfile? SenderDeveloperProfile { get; set; }
    public virtual MilestonePlanVersion? PlanVersion { get; set; }
    public virtual Milestone? Milestone { get; set; }
}
