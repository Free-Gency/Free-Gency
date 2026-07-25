using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class Wallet : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public owner OwnerType { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Available { get; set; } = 0;
    public decimal Reserved { get; set; } = 0;
    public decimal Pending { get; set; } = 0;

    public virtual User? OwnerUser { get; set; }
    public virtual Team? OwnerTeam { get; set; }
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = [];
}
