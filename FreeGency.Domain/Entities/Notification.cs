using FreeGency.Domain.Abstractions;

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

        // Scaller Properties


        // FKs


        // Navigation Properties
    }
}
