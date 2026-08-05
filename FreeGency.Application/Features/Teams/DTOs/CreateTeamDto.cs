using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public string? AboutUs { get; set; }
    public IFormFile? Logo { get; set; }
    public IFormFile? Cover { get; set; }
    public List<CategorySelectionDto> Categories { get; set; } = [];
    public List<Guid> SkillIds { get; set; } = [];
}

public sealed class CategorySelectionDto
{
    public Guid CategoryId { get; set; }
    public bool IsPrimary { get; set; }
}