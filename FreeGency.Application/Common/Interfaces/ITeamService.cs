using FreeGency.Application.Common.Pagination;
using FreeGency.Application.Features.Teams.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface ITeamService
{
    Task<ApiResponse<PaginatedResult<TeamDto>>> BrowseAsync(FilterTeamsRequestDto filter, CancellationToken ct = default);
    Task<ApiResponse<TeamDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TeamDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ApiResponse<TeamDto>> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateAsync(CreateTeamDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateTeamDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceCategoriesAsync(Guid teamId, UpdateTeamCategoriesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceSpecialtiesAsync(Guid teamId, UpdateTeamSpecialtiesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceSkillsAsync(Guid teamId, UpdateTeamSkillsDto dto, CancellationToken ct = default);
}
