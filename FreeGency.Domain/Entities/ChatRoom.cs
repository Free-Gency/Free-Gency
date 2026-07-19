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
    public Guid? TeamId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ProposalId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? Title { get; set; }

    public Team? Team { get; set; }
    public Project? Project { get; set; }
    public ProjectProposal? Proposal { get; set; }
    public User? CreatedByUser { get; set; }
    public bool IsReadOnly { get; set; } = false;
    public ICollection<ChatRoomMember> ChatRoomMembers { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
