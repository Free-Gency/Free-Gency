using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

/// <summary>
/// Tracks one private AI interviewer conversation between the HirePy AI and a single
/// accepted freelancer. The messages themselves live in the existing chat system
/// (ChatRoom + ChatRoomMember + Message) — this row only holds workflow state.
/// </summary>
public class HirePyInterview : ISoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid HirePySessionId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>The proposal the freelancer accepted through.</summary>
    public Guid ProjectProposalId { get; set; }

    /// <summary>Private chat room shared only by the freelancer and the HirePy AI.</summary>
    public Guid ChatRoomId { get; set; }

    /// <summary>The freelancer user (solo dev, or the accepting team leader).</summary>
    public Guid FreelancerUserId { get; set; }

    /// <summary>The freelancer's developer profile (the room's non-AI member).</summary>
    public Guid FreelancerDeveloperProfileId { get; set; }

    public HirePyInterviewStatus Status { get; set; } = HirePyInterviewStatus.Started;
    public int TurnCount { get; set; }

    /// <summary>Milestone-planning negotiation rounds already spent (bounds the AI revisions).</summary>
    public int PlanningRounds { get; set; }

    /// <summary>Id of the last user message the AI already answered (idempotency).</summary>
    public Guid? LastUserMessageId { get; set; }

    /// <summary>Set while an AI reply is being generated so a retry does not double-answer.</summary>
    public DateTime? ProcessingAt { get; set; }

    /// <summary>Consecutive AI-turn failures; the interview is failed after a bounded number.</summary>
    public int FailureCount { get; set; }

    public DateTime? ConcludedAt { get; set; }
    public string? FailReason { get; set; }
}
