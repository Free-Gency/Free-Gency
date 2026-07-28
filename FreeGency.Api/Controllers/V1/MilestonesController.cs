
namespace FreeGency.Api.Controllers.V1;


[Route("api/v1")]
[Authorize]
public class MilestonesController(IMilestoneService milestoneService) : BaseApiController
{
    [HttpGet("projects/{projectId:guid}/milestones")]
    public async Task<IActionResult> GetByProjectId([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await milestoneService.GetByProjectIdAsync(projectId, ct));
}