using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ClientNotificationSettings : ISoftDeletableEntity
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
        public bool NewMessageInApp { get; set; } = true;
        public bool NewMessageEmail { get; set; } = false;
        public bool ProposalReceivedInApp { get; set; } = true;
        public bool ProposalReceivedEmail { get; set; } = false;

        public bool MilestoneAddedInApp { get; set; } = true;
        public bool MilestoneAddedEmail { get; set; } = false;

        public bool WalletUpdatedInApp { get; set; } = true;
        public bool WalletUpdatedEmail { get; set; } = true;
        [ForeignKey(nameof(ProfileId))]
        public virtual ClientProfile clientProfile { get; set; } = null!;

    }
}
