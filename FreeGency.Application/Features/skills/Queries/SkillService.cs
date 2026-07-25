using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Extensions.QueryExtensions.Skills;
using FreeGency.Application.Common.Mappings.SkillsMapping;
using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Features.skills.Commands
{
    // Queries
    public partial class SkillService
    {
        public async Task<ApiResponse<PaginatedResult<SkillDto>>> BrowseAsync(
            FilterSkillsRequestDto filter,
            CancellationToken ct = default)
        {
            var query = _skillRepository.Query()
                .ApplyFilters(filter)
                .ApplySearch(filter.Search)
                .ApplySorting(filter);

            var page = await PaginatedResult<Skill>.CreateAsync(query, filter.PageNumber, filter.PageSize, ct);

            var mapped = PaginatedResult<SkillDto>.FromList(
                page.Items.Select(x => x.ToDto()).ToList(),
                page.PageNumber,
                page.PageSize,
                page.TotalCount);

            return ApiResponse.Success(mapped);
        }

        public async Task<ApiResponse<SkillDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var skill = await _skillRepository.GetByIdAsync(id, ct);

            if (skill is null)
                return ApiResponse.Failure<SkillDto>(AppError.NotFound(nameof(Skill), id));

            return ApiResponse.Success(skill.ToDto());
        }

        public async Task<ApiResponse<IEnumerable<SkillDto>>> SearchAsync(string query, int limit = 20, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return ApiResponse.Failure<IEnumerable<SkillDto>>(AppError.Validation("Search query is required."));

            var result = await BrowseAsync(new FilterSkillsRequestDto
            {
                Search = query.Trim(),
                PageNumber = 1,
                PageSize = limit
            }, ct);

            if (result.IsFailure)
                return ApiResponse.Failure<IEnumerable<SkillDto>>(result.Error!);

            return ApiResponse.Success(result.Data!.Items.AsEnumerable());
        }

        public async Task<ApiResponse<IEnumerable<SkillDto>>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        {
            if (!await _categoryRepository.ExistsAsync(categoryId, ct))
                return ApiResponse.Failure<IEnumerable<SkillDto>>(AppError.NotFound(nameof(Category), categoryId));

            var skills = await _skillRepository.GetByCategoryIdAsync(categoryId, ct);
            return ApiResponse.Success(skills.Select(x => x.ToDto()));
        }

        public async Task<ApiResponse<IEnumerable<SkillDto>>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken ct = default)
        {
            if (!await _specialtyRepository.ExistsAsync(specialtyId, ct))
                return ApiResponse.Failure<IEnumerable<SkillDto>>(AppError.NotFound(nameof(Specialty), specialtyId));

            var skills = await _skillRepository.GetBySpecialtyIdAsync(specialtyId, ct);
            return ApiResponse.Success(skills.Select(x => x.ToDto()));
        }
    }
}
