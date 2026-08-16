using FreeGency.AI.ProjectDrafting;
using FreeGency.Application.Common.DTOs.AIDtos;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Plans;
using FreeGency.Application.Features.Plans.Dtos;
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

    public ProjectDraftsController(
        ProjectDraftService draftService,
        IEntitlementService entitlementService,
        ICurrentUserService currentUser)
    {
        _draftService = draftService;
        _entitlementService = entitlementService;
        _currentUser = currentUser;
    }

    /// <summary>Soft gate before opening the AI drafting wizard (no quota consumed, no LLM call).</summary>
    [HttpGet("eligibility")]
    public async Task<IActionResult> GetEligibility(CancellationToken cancellationToken)
    {
        var create = await _entitlementService.CanConsumeAsync(
            _currentUser.UserId, FeatureType.CreateProject, cancellationToken);
        var draft = await _entitlementService.CanConsumeAsync(
            _currentUser.UserId, FeatureType.GenerateProjectDraft, cancellationToken);

        var dto = new ProjectDraftEligibilityDto
        {
            CanCreateProject = create.IsAllowed,
            CanGenerateDraft = draft.IsAllowed,
            CreateProject = Map(create),
            GenerateProjectDraft = Map(draft),
            Message = !create.IsAllowed
                ? (create.Message ?? create.ToAppError().message)
                : !draft.IsAllowed
                    ? (draft.Message ?? draft.ToAppError().message)
                    : null
        };

        return Ok(ApiResponse.Success(dto));
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ProjectDraftResponse>> Generate(
        [FromBody] GenerateProjectDraftRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserInput))
            return BadRequest("userInput is required.");

        var quota = await _entitlementService.CanConsumeAsync(
            _currentUser.UserId, FeatureType.GenerateProjectDraft, cancellationToken);
        if (!quota.IsAllowed)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = quota.ToAppError().message });

        try
        {
            var draft = await _draftService.GenerateDraftAsync(request.UserInput);
            await _entitlementService.ConsumeAsync(
                _currentUser.UserId, FeatureType.GenerateProjectDraft, cancellationToken);
            return Ok(draft);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Could not reach the ITI AI gateway. " + ex.Message });
        }
    }

    private static EntitlementCheckDto Map(EntitlementResult result) => new()
    {
        Feature = result.Feature.ToString(),
        IsAllowed = result.IsAllowed,
        IsEnabled = result.IsEnabled,
        Limit = result.Limit,
        Used = result.Used,
        Remaining = result.Remaining,
        PlanName = result.PlanName,
        Message = result.IsAllowed
            ? null
            : (result.Message ?? result.ToAppError().message)
    };
}
