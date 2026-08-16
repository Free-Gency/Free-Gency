using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Plans;
using FreeGency.Application.Features.Plans.Dtos;
using FreeGency.Domain.Entities.Plans;
using FreeGency.Infrastructure.Interfaces;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/entitlements")]
[ApiController]
[Authorize]
public class EntitlementsController(
    IEntitlementService entitlementService,
    ICurrentUserService currentUser) : BaseApiController
{
    /// <summary>Soft check — does not consume quota. Use before expensive AI calls.</summary>
    [HttpGet("{feature}/can-consume")]
    public async Task<IActionResult> CanConsume(string feature, CancellationToken ct)
    {
        if (!Enum.TryParse<FeatureType>(feature, ignoreCase: true, out var featureType))
            return BadRequest(ApiResponse.Failure(AppError.Validation($"Unknown feature '{feature}'.")));

        var result = await entitlementService.CanConsumeAsync(currentUser.UserId, featureType, ct);
        return Ok(ApiResponse.Success(Map(result)));
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
