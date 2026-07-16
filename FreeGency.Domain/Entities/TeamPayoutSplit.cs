using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class TeamPayoutSplit : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid TeamId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid UserId { get; set; }
    public SplitType SplitType { get; set; }
    public decimal Value { get; set; }

    public Team Team { get; set; } = null!;
    public Project? Project { get; set; }
    public User User { get; set; } = null!;
}
