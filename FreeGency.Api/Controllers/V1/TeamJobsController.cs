using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.TeamJobs.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1")]
public class TeamJobsController(ITeamJobService _teamJobService) : BaseApiController
{
    [HttpGet("jobs")]
    public async Task<IActionResult> Browse([FromQuery] FilterTeamJobsRequestDto filter, CancellationToken ct)
        => HandleResult(await _teamJobService.BrowseAsync(filter, ct));

    [HttpGet("jobs/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await _teamJobService.GetByIdAsync(id, ct));

    [HttpGet("teams/{teamId:guid}/jobs")]
    public async Task<IActionResult> GetByTeamId(Guid teamId, CancellationToken ct)
        => HandleResult(await _teamJobService.GetByTeamIdAsync(teamId, ct));

    [Authorize]
    [HttpPost("teams/{teamId:guid}/jobs")]
    public async Task<IActionResult> Create(Guid teamId, [FromBody] CreateTeamJobDto dto, CancellationToken ct)
        => HandleResult(await _teamJobService.CreateAsync(teamId, dto, ct));

    [Authorize]
    [HttpPut("jobs/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamJobDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return HandleResult(ApiResponse.Failure(AppError.Validation("Route id does not match body id.")));

        return HandleResult(await _teamJobService.UpdateAsync(dto, ct));
    }

    [Authorize]
    [HttpPut("jobs/{id:guid}/skills")]
    public async Task<IActionResult> UpdateSkills(Guid id, [FromBody] UpdateTeamJobSkillsDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return HandleResult(ApiResponse.Failure(AppError.Validation("Route id does not match body id.")));

        return HandleResult(await _teamJobService.UpdateSkillsAsync(dto, ct));
    }

    [Authorize]
    [HttpPost("jobs/{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
        => HandleResult(await _teamJobService.CloseAsync(id, ct));

    /// <summary>
    /// Backfill short/empty open job descriptions with a clearer role pitch.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("jobs/enrich-descriptions")]
    public async Task<IActionResult> EnrichDescriptions(CancellationToken ct)
        => HandleResult(await _teamJobService.EnrichWeakDescriptionsAsync(ct));
}
