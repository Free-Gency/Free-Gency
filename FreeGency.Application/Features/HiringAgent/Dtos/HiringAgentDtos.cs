using FreeGency.Domain.Enums;

namespace FreeGency.Application.Features.HiringAgent.Dtos;

public sealed class StartHiringAgentRunRequestDto
{
    public Guid ProjectId { get; init; }
    public int? TopK { get; init; }

    /// <summary>Hours to wait for invite accepts before expiring pending invites.</summary>
    public int? InviteWindowHours { get; init; }

    /// <summary>Hours from start until discussion/ranking must finish.</summary>
    public int? DiscussionWindowHours { get; init; }
}

public sealed class ConfirmHireRequestDto
{
    /// <summary>
    /// Optional. When set, hire this ranked candidate instead of the AI recommendation.
    /// </summary>
    public Guid? CandidateId { get; init; }
}

public sealed class HiringAgentRunDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public HiringAgentRunStatus Status { get; init; }
    public int TopK { get; init; }
    public DateTime InviteDeadlineUtc { get; init; }
    public DateTime DiscussionDeadlineUtc { get; init; }
    public Guid? RecommendedProposalId { get; init; }
    public Guid? RecommendedPlanVersionId { get; init; }
    public Guid? RecommendedCandidateId { get; init; }
    public string? FailureReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReportReadyAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? ClientHireApprovedAt { get; init; }
    public List<HiringAgentCandidateDto> Candidates { get; init; } = [];
}

public sealed class HiringAgentCandidateDto
{
    public Guid Id { get; init; }
    public ApplicantType InviteeType { get; init; }
    public Guid? InviteeUserId { get; init; }
    public Guid? InviteeTeamId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public float SuggestionScore { get; init; }
    public int RankOrder { get; init; }
    public HiringAgentCandidateStatus Status { get; init; }
    public Guid? InvitationId { get; init; }
    public Guid? ProposalId { get; init; }
    public Guid? ChatRoomId { get; init; }
    public Guid? LatestPlanVersionId { get; init; }
    public float? DiscussionScore { get; init; }
    public string? DiscussionNotes { get; init; }
    public int AgentMessageCount { get; init; }

    /// <summary>True when this person already had a proposal on the project (no Scout invite sent).</summary>
    public bool AlreadyApplied { get; init; }

    /// <summary>True when Scout's matcher recommended this candidate.</summary>
    public bool AiRecommended { get; init; }

    /// <summary>
    /// scout-invite | applied-and-recommended | existing-discussion
    /// </summary>
    public string SourceGroup { get; init; } = "scout-invite";
}

public sealed class HiringAgentReportDto
{
    public Guid RunId { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public HiringAgentRunStatus Status { get; init; }
    public string Summary { get; init; } = string.Empty;
    public HiringAgentCandidateDto? Recommended { get; init; }
    public Guid? RecommendedPlanVersionId { get; init; }
    public List<HiringAgentRankedDiscussionDto> RankedDiscussions { get; init; } = [];
    public List<string> Risks { get; init; } = [];
    public DateTime? ReportReadyAt { get; init; }
}

public sealed class HiringAgentRankedDiscussionDto
{
    public Guid CandidateId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public float Score { get; init; }
    public int Rank { get; init; }
    public string Summary { get; init; } = string.Empty;
    public List<string> Strengths { get; init; } = [];
    public List<string> Weaknesses { get; init; } = [];
    public Guid? ChatRoomId { get; init; }
    public Guid? PlanVersionId { get; init; }
    public bool HasMilestonePlan { get; init; }
}
