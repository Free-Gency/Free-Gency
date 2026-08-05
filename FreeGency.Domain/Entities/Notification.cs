using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities
{
    public class Notification : ISoftDeletableEntity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Scalar Properties
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public string? Data { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        // Owner: exactly one of ClientProfileId / DeveloperProfileId (XOR)
        public Guid? ClientProfileId { get; set; }
        public Guid? DeveloperProfileId { get; set; }

        // Context FKs
        public Guid? ProjectId { get; set; }
        public Guid? ProjectProposalId { get; set; }
        public Guid? TeamId { get; set; }
        public Guid? MilestoneId { get; set; }
        public Guid? ChatRoomId { get; set; }
        public Guid? MessageId { get; set; }

        // Navigation Properties
        public virtual ClientProfile? ClientProfile { get; set; }
        public virtual DeveloperProfile? DeveloperProfile { get; set; }
        public virtual Project? Project { get; set; }
        public virtual ProjectProposal? ProjectProposal { get; set; }
        public virtual Team? Team { get; set; }
        public virtual Milestone? Milestone { get; set; }
        public virtual ChatRoom? ChatRoom { get; set; }
        public virtual Message? Message { get; set; }
    }
}
