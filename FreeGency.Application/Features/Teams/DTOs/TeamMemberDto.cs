namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class TeamMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Job { get; set; }
    public bool IsOwner { get; set; }
    public string? JoinedAt { get; set; }
}

public sealed class UpdateTeamMemberRoleDto
{
    /// <summary>TeamLeader or TeamMember</summary>
    public string Role { get; set; } = string.Empty;
}
