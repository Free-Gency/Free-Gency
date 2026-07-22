namespace FreeGency.Application.Features.categories.Dtos;

public sealed class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? ImageCover { get; set; }
}
