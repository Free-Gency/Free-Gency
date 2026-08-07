using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class ChatRoomMember : ISoftDeletableEntity
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
    public Guid? ClientProfileId { get; set; }
    public Guid? DeveloperProfileId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public string? RoleLabel { get; set; }
    public bool CanSend { get; set; } = true;

    public virtual ChatRoom ChatRoom { get; set; } = null!;
    public virtual ClientProfile? ClientProfile { get; set; }
    public virtual DeveloperProfile? DeveloperProfile { get; set; }
}
