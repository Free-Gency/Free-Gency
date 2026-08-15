using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.HirePy.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/hirepy")]
public class HirePyController(
    IHirePySessionService hirePyService,
    IHirePyEvaluationService hirePyEvaluationService,
    IHirePyApprovalService hirePyApprovalService) : BaseApiController
{
    [Authorize]
    [HttpPost("sessions")]
    public async Task<IActionResult> Start(StartHirePySessionRequestDto request, CancellationToken ct)
        => HandleResult(await hirePyService.StartAsync(request, ct));

    [Authorize]
    [HttpGet("sessions/{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await hirePyService.GetAsync(id, ct));

    [Authorize]
    [HttpGet("sessions/mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => HandleResult(await hirePyService.GetMineAsync(ct));

    [Authorize]
    [HttpGet("sessions/{id:guid}/evaluations")]
    public async Task<IActionResult> GetEvaluations([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await hirePyEvaluationService.GetBySessionIdAsync(id, ct));

    [Authorize]
    [HttpGet("sessions/{id:guid}/recommendation")]
    public async Task<IActionResult> GetRecommendation([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await hirePyApprovalService.GetRecommendationAsync(id, ct));

    [Authorize]
    [HttpPost("sessions/{id:guid}/approve")]
    public async Task<IActionResult> ApproveAndHire([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await hirePyApprovalService.ApproveAndHireAsync(id, ct));
}
