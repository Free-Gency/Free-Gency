using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class PaymentTransaction : ISoftDeletableEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public Guid WalletId { get; set; }

        public string PaymentProviderRef { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentStatus Status { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? MilestoneId { get; set; }
        public string? FailureReason { get; set; }
        [ForeignKey(nameof(WalletId))]
        public virtual Wallet Wallet { get; set; } = null!;
        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; } = null!;
        [ForeignKey(nameof(MilestoneId))]
        public virtual Milestone Milestone { get; set; } = null!;
    }
    public enum PaymentStatus
    {
        Pending,
        Succeeded,
        Failed,
        Cancelled,
        Refunded
    }
}
