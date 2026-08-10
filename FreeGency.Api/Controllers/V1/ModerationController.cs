using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Moderation.DTOs;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/moderation")]
public class ModerationController(
    IContentModerationService moderationService,
    ICurrentUserService currentUser) : BaseApiController
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<MyModerationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyStatus(CancellationToken ct)
        => HandleResult(await moderationService.GetMyStatusAsync(currentUser.UserId, ct));

    [HttpGet("cases")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ModerationCaseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListOpenCases(CancellationToken ct)
        => HandleResult(await moderationService.ListOpenCasesAsync(ct));

    [HttpPost("cases/{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ResolveModerationCaseRequest? request,
        CancellationToken ct)
        => HandleResult(await moderationService.ResolveCaseAsync(
            id,
            request?.AdminNote,
            currentUser.UserId,
            ct));
}
