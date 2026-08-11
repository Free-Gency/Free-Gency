
namespace FreeGency.Application.Features.Teams.DTOs;


public sealed class TeamProjectCardDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public decimal BudgetMin { get; init; }
    public decimal BudgetMax { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime? Deadline { get; init; }
    public string? CategoryName { get; init; }
    public int TotalMilestones { get; init; }
    public int CompletedMilestones { get; init; }
    public int ProgressPercent { get; init; }
    public bool IsCurrentUserMember { get; init; }

    public string? CurrentMilestoneTitle { get; init; }
    public decimal? CurrentMilestoneAmount { get; init; }
    public string? CurrentMilestoneWorkStatus { get; init; }
    public DateTime? CurrentMilestoneDue { get; init; }
    public int CurrentMilestoneTasksDone { get; init; }
    public int CurrentMilestoneTasksTotal { get; init; }

    public IReadOnlyList<TeamProjectMemberAvatarDto> Members { get; init; } = [];
    public int MembersTotal { get; init; }
}


public sealed class TeamProjectMemberAvatarDto
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}


public sealed class ProjectMemberDto
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string RoleInProject { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
}


public sealed class AssignProjectMemberDto
{
    public Guid UserId { get; init; }
    public string RoleInProject { get; init; } = "Member";
}


public sealed class MilestoneAssignmentDto
{
    public Guid Id { get; init; }
    public Guid MilestoneId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal Percentage { get; init; }
}


public sealed class SetMilestoneAssignmentsDto
{
    public List<MilestoneAssignmentItemDto> Items { get; init; } = [];
}

public sealed class MilestoneAssignmentItemDto
{
    public Guid UserId { get; init; }
    public decimal Percentage { get; init; }
}


public sealed class MilestoneAssigneeDto
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}
