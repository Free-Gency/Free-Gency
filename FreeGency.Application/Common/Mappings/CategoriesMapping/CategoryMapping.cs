using FreeGency.Application.Features.categories.Dtos;

namespace FreeGency.Application.Common.Mappings.CategoriesMapping;

public static class CategoryMapping
{
    public static CategoryDto ToDto(this Category category)
        => new()
        {
            Id = category.Id,
            Name = category.Name,
            NameEn = category.NameEn,
            ImageCover = category.ImageCover
        };

    public static Category ToEntity(this CreateCategoryDto dto)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            NameEn = dto.NameEn,
            ImageCover = dto.ImageCover
        };
}
