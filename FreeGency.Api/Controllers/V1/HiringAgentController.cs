using FreeGency.Application.Features.HiringAgent.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/hiring-agent")]
[ApiController]
[Authorize]
public class HiringAgentController(IHiringAgentService hiringAgentService) : BaseApiController
{
    [HttpPost("runs")]
    public async Task<IActionResult> Start(
        [FromBody] StartHiringAgentRunRequestDto request,
        CancellationToken ct)
        => HandleResult(await hiringAgentService.StartAsync(request, ct));

    [HttpGet("runs")]
    public async Task<IActionResult> ListMine(CancellationToken ct)
        => HandleResult(await hiringAgentService.ListMineAsync(ct));

    [HttpGet("runs/{id:guid}")]
    public async Task<IActionResult> GetRun(Guid id, CancellationToken ct)
        => HandleResult(await hiringAgentService.GetRunAsync(id, ct));

    [HttpGet("runs/by-project/{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
        => HandleResult(await hiringAgentService.GetByProjectAsync(projectId, ct));

    [HttpGet("runs/{id:guid}/report")]
    public async Task<IActionResult> GetReport(Guid id, CancellationToken ct)
        => HandleResult(await hiringAgentService.GetReportAsync(id, ct));

    [HttpPost("runs/{id:guid}/confirm-hire")]
    public async Task<IActionResult> ConfirmHire(
        Guid id,
        [FromBody] ConfirmHireRequestDto? body,
        CancellationToken ct)
        => HandleResult(await hiringAgentService.ConfirmHireAsync(id, body?.CandidateId, ct));

    [HttpPost("runs/{id:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct)
        => HandleResult(await hiringAgentService.DismissAsync(id, ct));

    [HttpPost("runs/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        => HandleResult(await hiringAgentService.CancelAsync(id, ct));

    [HttpPost("runs/{id:guid}/close-invites")]
    public async Task<IActionResult> CloseInvites(Guid id, CancellationToken ct)
        => HandleResult(await hiringAgentService.CloseInvitesAsync(id, ct));
}
