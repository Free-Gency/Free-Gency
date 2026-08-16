using FreeGency.Application.Features.Milestones.DTOs;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1")]
[Authorize]
public class MilestonesController(IMilestoneService milestoneService) : BaseApiController
{
    [HttpGet("projects/{projectId:guid}/milestones")]
    public async Task<IActionResult> GetByProjectId([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await milestoneService.GetByProjectIdAsync(projectId, ct));

    [HttpGet("projects/{projectId:guid}/milestone-plans")]
    public async Task<IActionResult> GetPlanVersions([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await milestoneService.GetPlanVersionsAsync(projectId, ct));

    [HttpGet("projects/{projectId:guid}/milestone-plans/latest")]
    public async Task<IActionResult> GetLatestPlan([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await milestoneService.GetLatestPlanAsync(projectId, ct));

    [HttpPost("projects/{projectId:guid}/milestone-plans/ai-assist")]
    public async Task<IActionResult> AiAssistPlan(
        [FromRoute] Guid projectId,
        [FromBody] MilestonePlanAiAssistRequestDto request,
        CancellationToken ct,
        [FromServices] IMilestonePlanAiService planAi)
        => HandleResult(await planAi.AssistAsync(projectId, request, ct));

    [HttpGet("milestones/mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => HandleResult(await milestoneService.GetMyMilestonesAsync(ct));

    [HttpGet("milestones/{milestoneId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid milestoneId, CancellationToken ct)
        => HandleResult(await milestoneService.GetByIdAsync(milestoneId, ct));

    [HttpPost("milestone-plans")]
    public async Task<IActionResult> ProposePlan([FromBody] ProposeMilestonePlanDto dto, CancellationToken ct)
        => HandleResult(await milestoneService.ProposePlanAsync(dto, ct));

    [HttpPost("milestone-plans/request-changes")]
    public async Task<IActionResult> RequestChanges([FromBody] RequestPlanChangesDto dto, CancellationToken ct)
        => HandleResult(await milestoneService.RequestPlanChangesAsync(dto, ct));

    [HttpPost("milestone-plans/{planVersionId:guid}/accept")]
    public async Task<IActionResult> AcceptPlan([FromRoute] Guid planVersionId, CancellationToken ct)
        => HandleResult(await milestoneService.AcceptPlanAsync(planVersionId, ct));

    [HttpPost("projects/{projectId:guid}/milestones/fund-next")]
    public async Task<IActionResult> FundNext([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await milestoneService.FundNextMilestoneAsync(projectId, ct));

    [HttpPost("milestones/{milestoneId:guid}/submit")]
    public async Task<IActionResult> Submit(
        [FromRoute] Guid milestoneId,
        [FromBody] SubmitMilestoneBody? body,
        CancellationToken ct)
        => HandleResult(await milestoneService.SubmitMilestoneAsync(milestoneId, body?.Note, ct));

    [HttpPost("milestones/{milestoneId:guid}/approve-release")]
    public async Task<IActionResult> ApproveRelease([FromRoute] Guid milestoneId, CancellationToken ct)
        => HandleResult(await milestoneService.ApproveAndReleaseAsync(milestoneId, ct));

    [HttpPost("milestones/{milestoneId:guid}/request-work-changes")]
    public async Task<IActionResult> RequestWorkChanges(
        [FromRoute] Guid milestoneId,
        [FromBody] RequestWorkChangesBody body,
        CancellationToken ct)
        => HandleResult(await milestoneService.RequestMilestoneWorkChangesAsync(
            milestoneId, body?.Comment ?? string.Empty, ct));
}

public sealed class SubmitMilestoneBody
{
    public string? Note { get; init; }
}

public sealed class RequestWorkChangesBody
{
    public string? Comment { get; init; }
}
