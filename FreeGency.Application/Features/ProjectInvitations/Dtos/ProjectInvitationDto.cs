namespace FreeGency.Application.Features.ProjectInvitations.Dtos;

public sealed class ProjectInvitationDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public Guid ClientUserId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public ApplicantType InviteeType { get; init; }
    public Guid? InviteeUserId { get; init; }
    public string? InviteeUserName { get; init; }
    public Guid? InviteeTeamId { get; init; }
    public string? InviteeTeamName { get; init; }
    public string Message { get; init; } = string.Empty;
    public ProjectInvitationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? RespondedAt { get; init; }
    public Guid? ProposalId { get; init; }
    public Guid? ChatRoomId { get; init; }
}
