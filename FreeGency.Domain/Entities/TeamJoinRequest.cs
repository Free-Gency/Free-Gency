using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class TeamJoinRequest : ISoftDeletableEntity
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
    public Guid? TeamJobId { get; set; }
    public Guid UserId { get; set; }
    public string? CoverLetter { get; set; }
    public string? Job { get; set; }
    public TeamJoinRequestStatus Status { get; set; } = TeamJoinRequestStatus.pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? ResponseAt { get; set; }
    public string? RespondedByUserId { get; set; }

    public virtual Team Team { get; set; } = null!;
    public virtual TeamJob? TeamJob { get; set; }
    public virtual User User { get; set; } = null!;
}
