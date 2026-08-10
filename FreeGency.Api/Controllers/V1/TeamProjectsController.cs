
namespace FreeGency.Api.Controllers.V1;


[Route("api/v1")]
[Authorize]
public class TeamProjectsController(ITeamService teamService) : BaseApiController
{
    [HttpGet("teams/{teamId:guid}/projects")]
    public async Task<IActionResult> GetTeamProjects(Guid teamId, CancellationToken ct)
        => HandleResult(await teamService.GetTeamProjectsAsync(teamId, ct));


    [HttpGet("projects/{projectId:guid}/members")]
    public async Task<IActionResult> GetProjectMembers(Guid projectId, CancellationToken ct)
        => HandleResult(await teamService.GetProjectMembersAsync(projectId, ct));


    [HttpPost("projects/{projectId:guid}/members")]
    public async Task<IActionResult> AssignProjectMember(Guid projectId, [FromBody] AssignProjectMemberDto dto, CancellationToken ct)
        => HandleResult(await teamService.AssignProjectMemberAsync(projectId, dto, ct));


    [HttpDelete("projects/{projectId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveProjectMember(Guid projectId, Guid userId, CancellationToken ct)
        => HandleResult(await teamService.RemoveProjectMemberAsync(projectId, userId, ct));


    [HttpGet("milestones/{milestoneId:guid}/assignments")]
    public async Task<IActionResult> GetMilestoneAssignments(Guid milestoneId, CancellationToken ct)
        => HandleResult(await teamService.GetMilestoneAssignmentsAsync(milestoneId, ct));


    [HttpPut("milestones/{milestoneId:guid}/assignments")]
    public async Task<IActionResult> SetMilestoneAssignments(Guid milestoneId, [FromBody] SetMilestoneAssignmentsDto dto, CancellationToken ct)
        => HandleResult(await teamService.SetMilestoneAssignmentsAsync(milestoneId, dto, ct));


    [HttpGet("milestones/{milestoneId:guid}/assignees")]
    public async Task<IActionResult> GetMilestoneAssignees(Guid milestoneId, CancellationToken ct)
        => HandleResult(await teamService.GetMilestoneAssigneesAsync(milestoneId, ct));
}
