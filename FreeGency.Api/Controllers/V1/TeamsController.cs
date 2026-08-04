using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Teams.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/teams")]
public class TeamsController(ITeamService _teamService) : BaseApiController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] CreateTeamDto dto, CancellationToken ct)
        => HandleResult(await _teamService.CreateAsync(dto, ct));

    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] FilterTeamsRequestDto filter, CancellationToken ct)
        => HandleResult(await _teamService.BrowseAsync(filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await _teamService.GetByIdAsync(id, ct));

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => HandleResult(await _teamService.GetMineAsync(ct));

    [HttpGet("by-code/{teamCode}")]
    public async Task<IActionResult> GetByTeamCode(string teamCode, CancellationToken ct)
        => HandleResult(await _teamService.GetByTeamCodeAsync(teamCode, ct));

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateTeamDto dto, CancellationToken ct)
    {
        dto.Id = id;
        return HandleResult(await _teamService.UpdateAsync(dto, ct));
    }

    [HttpPut("{id:guid}/categories")]
    [Authorize]
    public async Task<IActionResult> ReplaceCategories(Guid id, [FromBody] UpdateTeamCategoriesDto dto, CancellationToken ct)
        => HandleResult(await _teamService.ReplaceCategoriesAsync(id, dto, ct));

    [HttpPut("{id:guid}/specialties")]
    [Authorize]
    public async Task<IActionResult> ReplaceSpecialties(Guid id, [FromBody] UpdateTeamSpecialtiesDto dto, CancellationToken ct)
        => HandleResult(await _teamService.ReplaceSpecialtiesAsync(id, dto, ct));

    [HttpPut("{id:guid}/skills")]
    [Authorize]
    public async Task<IActionResult> ReplaceSkills(Guid id, [FromBody] UpdateTeamSkillsDto dto, CancellationToken ct)
        => HandleResult(await _teamService.ReplaceSkillsAsync(id, dto, ct));
}