using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.TeamJobs.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface ITeamJobService
{
    Task<ApiResponse<PaginatedResult<TeamJobDto>>> BrowseAsync(FilterTeamJobsRequestDto filter, CancellationToken ct = default);
    Task<ApiResponse<TeamJobDetailsDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TeamJobDto>>> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateAsync(Guid teamId, CreateTeamJobDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateTeamJobDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateSkillsAsync(UpdateTeamJobSkillsDto dto, CancellationToken ct = default);
    Task<ApiResponse> CloseAsync(Guid id, CancellationToken ct = default);
}
