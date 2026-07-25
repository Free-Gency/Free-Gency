using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface ISkillService
{
    Task<ApiResponse<PaginatedResult<SkillDto>>> BrowseAsync(FilterSkillsRequestDto filter, CancellationToken ct = default);
    Task<ApiResponse<SkillDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SkillDto>>> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SkillDto>>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<SkillDto>>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateAsync(CreateSkillDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateSkillDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}
