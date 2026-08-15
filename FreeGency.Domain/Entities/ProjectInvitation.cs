using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class ProjectInvitation : ISoftDeletableEntity
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
    public Guid ClientUserId { get; set; }

    /// <summary>Reuses ApplicantType: User = solo developer, Team = agency/team.</summary>
    public ApplicantType InviteeType { get; set; }
    public Guid? InviteeUserId { get; set; }
    public Guid? InviteeTeamId { get; set; }

    public string Message { get; set; } = string.Empty;
    public ProjectInvitationStatus Status { get; set; } = ProjectInvitationStatus.Pending;

    public DateTime? ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public Guid? RespondedByUserId { get; set; }

    public Guid? ProposalId { get; set; }
    public Guid? ChatRoomId { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual User ClientUser { get; set; } = null!;
    public virtual User? InviteeUser { get; set; }
    public virtual Team? InviteeTeam { get; set; }
    public virtual ProjectProposal? Proposal { get; set; }
    public virtual ChatRoom? ChatRoom { get; set; }
}
