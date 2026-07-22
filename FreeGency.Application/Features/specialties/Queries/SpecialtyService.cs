using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Extensions.QueryExtensions.Specialties;
using FreeGency.Application.Common.Mappings.SpecialtiesMapping;
using FreeGency.Application.Common.Mappings.SkillsMapping;
using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Features.specialties.Commands
{
    // Queries
    public partial class SpecialtyService
    {
        public async Task<ApiResponse<PaginatedResult<SpecialtyDto>>> BrowseAsync(
            FilterSpecialtiesRequestDto filter,
            CancellationToken ct = default)
        {
            var query = _specialtyRepository.Query()
                .ApplyFilters(filter)
                .ApplySearch(filter.Search)
                .ApplySorting(filter);

            var page = await PaginatedResult<Specialty>.CreateAsync(query, filter.PageNumber, filter.PageSize, ct);

            var mapped = PaginatedResult<SpecialtyDto>.FromList(
                page.Items.Select(x => x.ToDto()).ToList(),
                page.PageNumber,
                page.PageSize,
                page.TotalCount);

            return ApiResponse.Success(mapped);
        }

        public async Task<ApiResponse<SpecialtyDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id, ct);

            if (specialty is null)
                return ApiResponse.Failure<SpecialtyDto>(AppError.NotFound(nameof(Specialty), id));

            return ApiResponse.Success(specialty.ToDto());
        }

        public async Task<ApiResponse<IEnumerable<SpecialtyDto>>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        {
            if (!await _categoryRepository.ExistsAsync(categoryId, ct))
                return ApiResponse.Failure<IEnumerable<SpecialtyDto>>(AppError.SpecialtyInvalidCategory(categoryId));

            var specialties = await _specialtyRepository.GetByCategoryIdAsync(categoryId, ct);
            return ApiResponse.Success(specialties.Select(x => x.ToDto()));
        }

        public async Task<ApiResponse<IEnumerable<SkillDto>>> GetSkillsAsync(Guid specialtyId, CancellationToken ct = default)
        {
            if (!await _specialtyRepository.ExistsAsync(specialtyId, ct))
                return ApiResponse.Failure<IEnumerable<SkillDto>>(AppError.NotFound(nameof(Specialty), specialtyId));

            var skills = await _skillRepository.GetBySpecialtyIdAsync(specialtyId, ct);
            return ApiResponse.Success(skills.Select(x => x.ToDto()));
        }
    }
}
