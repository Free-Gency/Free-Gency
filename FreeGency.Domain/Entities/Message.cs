using FreeGency.Domain.Abstractions;

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
    public Guid? SenderUserId { get; set; }
    public string? Text { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;
    public User? SenderUser { get; set; }
}
