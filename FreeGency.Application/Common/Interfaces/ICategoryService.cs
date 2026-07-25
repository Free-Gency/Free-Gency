using FreeGency.Application.Features.categories.Dtos;
using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<PaginatedResult<CategoryDto>>> BrowseAsync(FilterCategoriesRequestDto filter, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SpecialtyDto>>> GetSpecialtiesAsync(Guid categoryId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SkillDto>>> GetSkillsAsync(Guid categoryId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}
