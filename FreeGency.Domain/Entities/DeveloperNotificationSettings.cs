using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class DeveloperNotificationSettings : ISoftDeletableEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public Guid ProfileId { get; set; }

        // Messages
        public bool MessagesInApp { get; set; } = true;
        public bool MessagesEmail { get; set; } = true;

        // Projects / Proposals
        public bool ProjectsInApp { get; set; } = true;
        public bool ProjectsEmail { get; set; } = false;

        // Milestones
        public bool MilestonesInApp { get; set; } = true;
        public bool MilestonesEmail { get; set; } = true;

        // Wallet / Payments
        public bool WalletInApp { get; set; } = true;
        public bool WalletEmail { get; set; } = true;

        // Teams
        public bool TeamsInApp { get; set; } = true;
        public bool TeamsEmail { get; set; } = false;
        [ForeignKey(nameof(ProfileId))]
        public virtual DeveloperProfile developerProfile { get; set; } = null!;
    }
}
