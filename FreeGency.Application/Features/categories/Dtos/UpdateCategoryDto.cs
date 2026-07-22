using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.categories.Dtos;

public sealed class UpdateCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public IFormFile? ImageCover { get; set; }
}