using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class HiringAgentCandidate : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid HiringAgentRunId { get; set; }

    public ApplicantType InviteeType { get; set; }
    public Guid? InviteeUserId { get; set; }
    public Guid? InviteeTeamId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public float SuggestionScore { get; set; }
    public int RankOrder { get; set; }

    public HiringAgentCandidateStatus Status { get; set; } = HiringAgentCandidateStatus.Suggested;

    public Guid? InvitationId { get; set; }
    public Guid? ProposalId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public Guid? LatestPlanVersionId { get; set; }

    public float? DiscussionScore { get; set; }
    public string? DiscussionNotes { get; set; }
    public int AgentMessageCount { get; set; }
    public DateTime? LastAgentMessageAt { get; set; }

    public virtual HiringAgentRun HiringAgentRun { get; set; } = null!;
    public virtual ProjectInvitation? Invitation { get; set; }
    public virtual ProjectProposal? Proposal { get; set; }
    public virtual ChatRoom? ChatRoom { get; set; }
    public virtual MilestonePlanVersion? LatestPlanVersion { get; set; }
}
