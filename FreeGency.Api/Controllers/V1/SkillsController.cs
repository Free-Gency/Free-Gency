using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/skills")]
public class SkillsController(ISkillService _skillService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] FilterSkillsRequestDto filter, CancellationToken ct)
        => HandleResult(await _skillService.BrowseAsync(filter, ct));

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20, CancellationToken ct = default)
        => HandleResult(await _skillService.SearchAsync(q, limit, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await _skillService.GetByIdAsync(id, ct));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSkillDto dto, CancellationToken ct)
        => HandleResult(await _skillService.CreateAsync(dto, ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSkillDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return HandleResult(ApiResponse.Failure(AppError.Validation("Route id does not match body id.")));

        return HandleResult(await _skillService.UpdateAsync(dto, ct));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => HandleResult(await _skillService.DeleteAsync(id, ct));
}
