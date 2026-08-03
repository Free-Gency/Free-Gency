
namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/team-suggestions")]
[ApiController]
[Authorize]
public class TeamSuggestionsController(ITeamSuggestionService teamSuggestionService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetForDeveloper(CancellationToken ct, [FromQuery] int topK = 10)
        => HandleResult(await teamSuggestionService.SuggestTeamsForDeveloperAsync(topK, ct));
}
