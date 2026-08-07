namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class ChatRoomDto
    {
        public Guid Id { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public Guid? TeamId { get; set; }
        public string? LastMessage { get; set; }
        public string? LastMessageType { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessageSender { get; set; }
        public DateTime? ArchivedAt { get; set; }
        /// <summary>Whether the current active profile can send in this room.</summary>
        public bool CanSend { get; set; }
        public string? RoleLabel { get; set; }
        public string? TeamName { get; set; }
        public string? TeamLogo { get; set; }
        /// <summary>Room-specific avatar (TeamMain / TeamGroup).</summary>
        public string? Logo { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ProposalId { get; set; }
        /// <summary>Other 1:1 peer profile id (null for group rooms).</summary>
        public Guid? OtherProfileId { get; set; }
    }
}
