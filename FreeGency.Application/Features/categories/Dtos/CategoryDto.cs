namespace FreeGency.Application.Features.categories.Dtos;

public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? ImageCover { get; set; }
}
