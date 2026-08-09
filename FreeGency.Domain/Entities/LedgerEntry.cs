using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class LedgerEntry : ISoftDeletableEntity
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
    public EntryType EntryType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? PaymentProviderRef { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual Milestone? Milestone { get; set; }
}