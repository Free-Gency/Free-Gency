namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class ChatRoomFilter : PagedQuery
    {
        public RoomType? RoomType { get; set; }
        public ChatRoomStatus? Status { get; set; }
        public string? Search { get; set; }
        /// <summary>When set, only rooms belonging to this team (Proposal / Project / TeamGroup / TeamMain).</summary>
        public Guid? TeamId { get; set; }
    }
}
