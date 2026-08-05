using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class ChatRoom : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public RoomType RoomType { get; set; }
    public ChatRoomStatus Status { get; set; } = ChatRoomStatus.Active;
    public DateTime? ArchivedAt { get; set; }
    public Guid? SourceProposalRoomId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ProposalId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? Title { get; set; }
    /// <summary>Optional room avatar (TeamMain / TeamGroup). Falls back to team logo in UI when null.</summary>
    public string? Logo { get; set; }

    public virtual Team? Team { get; set; }
    public virtual Project? Project { get; set; }
    public virtual ProjectProposal? Proposal { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual ChatRoom? SourceProposalRoom { get; set; }
    public virtual ICollection<ChatRoomMember> ChatRoomMembers { get; set; } = [];
    public virtual ICollection<Message> Messages { get; set; } = [];
}
