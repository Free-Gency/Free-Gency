using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Api.Controllers.V1;

[Route("api/v1/categories")]
public class CategoriesController(ICategoryService _categoryService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] FilterCategoriesRequestDto filter, CancellationToken ct)
        => HandleResult(await _categoryService.BrowseAsync(filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await _categoryService.GetByIdAsync(id, ct));

    [HttpGet("{id:guid}/specialties")]
    public async Task<IActionResult> GetSpecialties(Guid id, CancellationToken ct)
        => HandleResult(await _categoryService.GetSpecialtiesAsync(id, ct));

    [HttpGet("{id:guid}/skills")]
    public async Task<IActionResult> GetSkills(Guid id, CancellationToken ct)
        => HandleResult(await _categoryService.GetSkillsAsync(id, ct));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto, CancellationToken ct)
        => HandleResult(await _categoryService.CreateAsync(dto, ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCategoryDto dto, CancellationToken ct)
    {
        dto.Id = id;
        return HandleResult(await _categoryService.UpdateAsync(dto, ct));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => HandleResult(await _categoryService.DeleteAsync(id, ct));
}
