using FreeGency.AI.ProjectDrafting;
using FreeGency.Application.Common.DTOs.AIDtos;
using FreeGency.Domain.Entities.Plans;
using FreeGency.Infrastructure.Interfaces;


namespace FreeGency.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/project-drafts")]
public class ProjectDraftsController : ControllerBase
{
    private readonly ProjectDraftService _draftService;
    private readonly IEntitlementService _entitlementService;
    private readonly ICurrentUserService _currentUser;

    public ProjectDraftsController(ProjectDraftService draftService, IEntitlementService entitlementService, ICurrentUserService currentUser)
    {
        _draftService = draftService;
        _entitlementService = entitlementService;
        _currentUser = currentUser;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ProjectDraftResponse>> Generate([FromBody] GenerateProjectDraftRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserInput))
            return BadRequest("userInput is required.");

        var quota = await _entitlementService.CanConsumeAsync(_currentUser.UserId, FeatureType.GenerateProjectDraft, cancellationToken);
        if (!quota.IsAllowed)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = quota.ToAppError().message });

        try
        {
            var draft = await _draftService.GenerateDraftAsync(request.UserInput);
            await _entitlementService.ConsumeAsync(_currentUser.UserId, FeatureType.GenerateProjectDraft, cancellationToken);
            return Ok(draft);
        }
        catch (InvalidOperationException ex) { return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message }); }
        catch (HttpRequestException ex) { return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not reach the ITI AI gateway. " + ex.Message }); }
    }
}
