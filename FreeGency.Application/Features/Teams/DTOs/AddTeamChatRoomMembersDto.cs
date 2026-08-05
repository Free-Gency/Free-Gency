namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class AddTeamChatRoomMembersDto
{
    public List<Guid> MemberUserIds { get; set; } = [];
}
