
namespace FreeGency.Api.Controllers.V1;


[Route("api/v1")]
[Authorize]
public class EscrowController(IEscrowService escrowService) : BaseApiController
{
    [HttpGet("projects/{projectId:guid}/escrow")]
    public async Task<IActionResult> GetByProjectId([FromRoute] Guid projectId, CancellationToken ct)
        => HandleResult(await escrowService.GetByProjectIdAsync(projectId, ct));
}
