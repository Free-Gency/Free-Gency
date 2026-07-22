using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface ISpecialtyService
{
    Task<ApiResponse<PaginatedResult<SpecialtyDto>>> BrowseAsync(FilterSpecialtiesRequestDto filter, CancellationToken ct = default);
    Task<ApiResponse<SpecialtyDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SpecialtyDto>>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SkillDto>>> GetSkillsAsync(Guid specialtyId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateAsync(CreateSpecialtyDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateSpecialtyDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}
