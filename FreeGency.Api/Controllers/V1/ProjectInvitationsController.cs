using FreeGency.Application.Features.ProjectInvitations.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/project-invitations")]
[ApiController]
[Authorize]
public class ProjectInvitationsController(IProjectInvitationService invitationService) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectInvitationDto dto, CancellationToken ct)
        => HandleResult(await invitationService.CreateAsync(dto, ct));

    [HttpGet("sent")]
    public async Task<IActionResult> GetSent([FromQuery] FilterProjectInvitationsDto filter, CancellationToken ct)
        => HandleResult(await invitationService.GetSentAsync(filter, ct));

    [HttpGet("received")]
    public async Task<IActionResult> GetReceived([FromQuery] FilterProjectInvitationsDto filter, CancellationToken ct)
        => HandleResult(await invitationService.GetReceivedAsync(filter, ct));

    [HttpGet("teams/{teamId:guid}")]
    public async Task<IActionResult> GetForTeam(
        Guid teamId,
        [FromQuery] FilterProjectInvitationsDto filter,
        CancellationToken ct)
        => HandleResult(await invitationService.GetForTeamAsync(teamId, filter, ct));

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
        => HandleResult(await invitationService.AcceptAsync(id, ct));

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
        => HandleResult(await invitationService.RejectAsync(id, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        => HandleResult(await invitationService.CancelAsync(id, ct));
}
