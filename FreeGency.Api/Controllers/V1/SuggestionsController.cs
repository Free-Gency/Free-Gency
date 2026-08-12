using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Suggestions.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/suggestions")]
public class SuggestionsController(ISuggestionService suggestionService) : BaseApiController
{
    /// <summary>
    /// Suggest open team jobs/teams that match the current developer's profile.
    /// </summary>
    [HttpGet("teams-for-me")]
    [ProducesResponseType(typeof(ApiResponse<TeamsForMeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestTeamsForMe([FromQuery] int topK = 10, CancellationToken ct = default)
        => HandleResult(await suggestionService.SuggestTeamsForMeAsync(topK, ct));

    /// <summary>
    /// Suggest teams and developers that match a client's published project.
    /// </summary>
    [HttpGet("candidates-for-project/{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectCandidatesResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestCandidatesForProject(
        Guid projectId,
        [FromQuery] int topK = 10,
        CancellationToken ct = default)
        => HandleResult(await suggestionService.SuggestCandidatesForProjectAsync(projectId, topK, ct));

    /// <summary>
    /// Full reindex of developers, teams, open jobs, and open projects into Qdrant.
    /// Available to any authenticated user.
    /// </summary>
    [HttpPost("reindex")]
    [ProducesResponseType(typeof(ApiResponse<ReindexResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reindex(CancellationToken ct = default)
        => HandleResult(await suggestionService.ReindexAllAsync(ct));
}
