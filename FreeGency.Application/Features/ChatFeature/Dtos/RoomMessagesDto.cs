namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class RoomMessagesDto
    {
        public Guid Id { get; set; }
        public Guid ChatRoomId { get; set; }
        public Guid? SenderId { get; set; }
        public string? SenderProfileType { get; set; }
        public string? SenderName { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public Guid? PlanVersionId { get; set; }
        public Guid? MilestoneId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMine { get; set; }
        public Guid? OtherProfileId { get; set; }
        public string ModerationStatus { get; set; } = "Visible";
        public string? ModerationWarning { get; set; }
        public bool IsAgentGenerated { get; set; }
    }
}
