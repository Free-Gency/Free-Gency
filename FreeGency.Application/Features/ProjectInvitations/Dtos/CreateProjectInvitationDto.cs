namespace FreeGency.Application.Features.ProjectInvitations.Dtos;

public sealed class CreateProjectInvitationDto
{
    public Guid ProjectId { get; init; }
    public ApplicantType InviteeType { get; init; }
    public Guid? InviteeUserId { get; init; }
    public Guid? InviteeTeamId { get; init; }
    public string Message { get; init; } = string.Empty;
}
