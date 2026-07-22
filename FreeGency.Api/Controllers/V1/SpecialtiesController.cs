using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.specialties.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FreeGency.Api.Controllers;

[ApiController]
[Route("api/v1/specialties")]
public class SpecialtiesController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialtiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _specialtyService.GetAllAsync(ct);

        if (result.isFailure)
            return StatusCode(result.error.StatusCode ?? StatusCodes.Status500InternalServerError, result.error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _specialtyService.GetByIdAsync(id, ct);

        if (result.isFailure)
            return StatusCode(result.error.StatusCode ?? StatusCodes.Status500InternalServerError, result.error);

        return Ok(result.Value);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken ct)
    {
        var result = await _specialtyService.GetByCategoryIdAsync(categoryId, ct);

        if (result.isFailure)
            return StatusCode(result.error.StatusCode ?? StatusCodes.Status500InternalServerError, result.error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto, CancellationToken ct)
    {
        var result = await _specialtyService.CreateAsync(dto, ct);

        if (result.isFailure)
            return StatusCode(result.error.StatusCode ?? StatusCodes.Status500InternalServerError, result.error);

        return Ok();
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateSpecialtyDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return BadRequest();

        var result = await _specialtyService.UpdateAsync(dto, ct);

        if (result.isFailure)
            return StatusCode(result.error.StatusCode ?? StatusCodes.Status500InternalServerError, result.error);

        return NoContent();
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _specialtyService.DeleteAsync(id, ct);

        if (result.isFailure)
            return StatusCode(result.error.StatusCode ?? StatusCodes.Status500InternalServerError, result.error);

        return NoContent();
    }
}