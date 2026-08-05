namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class CreateTeamGroupDto
{
    public string Title { get; set; } = string.Empty;
    /// <summary>Optional member user ids to include. Creator is always included.</summary>
    public List<Guid> MemberUserIds { get; set; } = [];
}
