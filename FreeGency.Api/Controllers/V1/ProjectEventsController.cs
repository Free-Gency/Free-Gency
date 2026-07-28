
namespace FreeGency.Api.Controllers.V1;


[Route("api/v1")]
[Authorize]
public class ProjectEventsController(IProjectEventService projectEventService) : BaseApiController
{
    [HttpGet("projects/{projectId:guid}/events")]
    public async Task<IActionResult> GetByProjectId(
        [FromRoute] Guid projectId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
        => HandleResult(await projectEventService.GetByProjectIdAsync(projectId, skip, take, ct));



}