namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class TeamChatRoomMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoleLabel { get; set; }
    public bool CanSend { get; set; }
}
