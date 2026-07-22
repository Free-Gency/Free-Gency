using FreeGency.AI.ProjectDrafting;
using FreeGency.Application.Common.DTOs.AIDtos;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectDraftsController : ControllerBase
{
    private readonly ProjectDraftService _draftService;

    public ProjectDraftsController(ProjectDraftService draftService)
    {
        _draftService = draftService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ProjectDraftResponse>> Generate(
        [FromBody] GenerateProjectDraftRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserInput))
            return BadRequest("userInput is required.");

        var draft = await _draftService.GenerateDraftAsync(request.UserInput);
        return Ok(draft);
    }
}