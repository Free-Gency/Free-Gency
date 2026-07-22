using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Extensions.QueryExtensions.Categories;
using FreeGency.Application.Common.Mappings.CategoriesMapping;
using FreeGency.Application.Common.Mappings.SkillsMapping;
using FreeGency.Application.Common.Mappings.SpecialtiesMapping;
using FreeGency.Application.Features.categories.Dtos;
using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Features.categories;

public partial class CategoryService
{
    public async Task<ApiResponse<PaginatedResult<CategoryDto>>> BrowseAsync(
        FilterCategoriesRequestDto filter,
        CancellationToken ct = default)
    {
        var query = _categoryRepository.Query()
            .ApplyFilters(filter)
            .ApplySearch(filter.Search)
            .ApplySorting(filter);

        var page = await PaginatedResult<Category>.CreateAsync(query, filter.PageNumber, filter.PageSize, ct);

        var mapped = PaginatedResult<CategoryDto>.FromList(
            page.Items.Select(x => x.ToDto()).ToList(),
            page.PageNumber,
            page.PageSize,
            page.TotalCount);

        return ApiResponse.Success(mapped);
    }

    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);

        if (category is null)
            return ApiResponse.Failure<CategoryDto>(AppError.NotFound(nameof(Category), id));

        return ApiResponse.Success(category.ToDto());
    }

    public async Task<ApiResponse<IEnumerable<SpecialtyDto>>> GetSpecialtiesAsync(Guid categoryId, CancellationToken ct = default)
    {
        if (!await _categoryRepository.ExistsAsync(categoryId, ct))
            return ApiResponse.Failure<IEnumerable<SpecialtyDto>>(AppError.NotFound(nameof(Category), categoryId));

        var specialties = await _specialtyRepository.GetByCategoryIdAsync(categoryId, ct);
        return ApiResponse.Success(specialties.Select(x => x.ToDto()));
    }

    public async Task<ApiResponse<IEnumerable<SkillDto>>> GetSkillsAsync(Guid categoryId, CancellationToken ct = default)
    {
        if (!await _categoryRepository.ExistsAsync(categoryId, ct))
            return ApiResponse.Failure<IEnumerable<SkillDto>>(AppError.NotFound(nameof(Category), categoryId));

        var skills = await _skillRepository.GetByCategoryIdAsync(categoryId, ct);
        return ApiResponse.Success(skills.Select(x => x.ToDto()));
    }
}
