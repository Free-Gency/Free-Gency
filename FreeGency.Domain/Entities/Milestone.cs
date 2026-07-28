using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class Milestone : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ReleasedAmount { get; set; } = 0;
    public int SortOrder { get; set; } = 0;
    public ReleaseStatus ReleaseStatus { get; set; } = ReleaseStatus.Locked;
    public WorkStatus WorkStatus { get; set; } = WorkStatus.NotStarted;
    public string? ProposedByUserId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AvailableAt { get; set; }
    public DateTime? ReleasedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<ProjectFile> ProjectFiles { get; set; } = [];
    public virtual ICollection<ProjectEvent> ProjectEvents { get; set; } = [];
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = [];
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];
}
