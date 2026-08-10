using FreeGency.Application.Features.PayoutSplits.DTOs;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1")]
[Authorize]
public class PayoutSplitsController(IPayoutSplitService payoutSplitService) : BaseApiController
{
    [HttpGet("teams/{teamId:guid}/payout-splits")]
    public async Task<IActionResult> GetTeamDefaults([FromRoute] Guid teamId, CancellationToken ct)
        => HandleResult(await payoutSplitService.GetTeamDefaultsAsync(teamId, ct));

    [HttpPut("teams/{teamId:guid}/payout-splits")]
    public async Task<IActionResult> ReplaceTeamDefaults(
        [FromRoute] Guid teamId,
        [FromBody] ReplacePayoutSplitsDto dto,
        CancellationToken ct)
        => HandleResult(await payoutSplitService.ReplaceTeamDefaultsAsync(teamId, dto, ct));

    [HttpGet("projects/{projectId:guid}/payout-splits")]
    public async Task<IActionResult> GetProjectSplits([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await payoutSplitService.GetProjectSplitsAsync(projectId, ct));

    [HttpPut("projects/{projectId:guid}/payout-splits")]
    public async Task<IActionResult> ReplaceProjectSplits(
        [FromRoute] Guid projectId,
        [FromBody] ReplacePayoutSplitsDto dto,
        CancellationToken ct)
        => HandleResult(await payoutSplitService.ReplaceProjectSplitsAsync(projectId, dto, ct));

    [HttpGet("milestones/{milestoneId:guid}/payout-splits")]
    public async Task<IActionResult> GetMilestoneSplits([FromRoute] Guid milestoneId, CancellationToken ct)
        => HandleResult(await payoutSplitService.GetMilestoneSplitsAsync(milestoneId, ct));

    [HttpPut("milestones/{milestoneId:guid}/payout-splits")]
    public async Task<IActionResult> ReplaceMilestoneSplits(
        [FromRoute] Guid milestoneId,
        [FromBody] ReplacePayoutSplitsDto dto,
        CancellationToken ct)
        => HandleResult(await payoutSplitService.ReplaceMilestoneSplitsAsync(milestoneId, dto, ct));
}
