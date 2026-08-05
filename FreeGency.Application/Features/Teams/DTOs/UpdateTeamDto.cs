using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class UpdateTeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AboutUs { get; set; }
    public IFormFile? Logo { get; set; }
    public IFormFile? Cover { get; set; }
}