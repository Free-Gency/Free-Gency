using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/specialties")]
public class SpecialtiesController(ISpecialtyService _specialtyService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] FilterSpecialtiesRequestDto filter, CancellationToken ct)
        => HandleResult(await _specialtyService.BrowseAsync(filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await _specialtyService.GetByIdAsync(id, ct));

    [HttpGet("{id:guid}/skills")]
    public async Task<IActionResult> GetSkills(Guid id, CancellationToken ct)
        => HandleResult(await _specialtyService.GetSkillsAsync(id, ct));

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken ct)
        => HandleResult(await _specialtyService.GetByCategoryIdAsync(categoryId, ct));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto, CancellationToken ct)
        => HandleResult(await _specialtyService.CreateAsync(dto, ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialtyDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return HandleResult(ApiResponse.Failure(AppError.Validation("Route id does not match body id.")));

        return HandleResult(await _specialtyService.UpdateAsync(dto, ct));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => HandleResult(await _specialtyService.DeleteAsync(id, ct));
}
