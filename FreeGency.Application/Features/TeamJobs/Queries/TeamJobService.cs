using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Extensions.QueryExtensions.TeamJobs;
using FreeGency.Application.Common.Mappings.SkillsMapping;
using FreeGency.Application.Common.Mappings.TeamJobsMapping;
using FreeGency.Application.Features.skills.Dtos;
using FreeGency.Application.Features.TeamJobs.Dtos;
using FreeGency.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Application.Features.TeamJobs.Commands;

// Queries
public partial class TeamJobService
{
    public async Task<ApiResponse<PaginatedResult<TeamJobDto>>> BrowseAsync(
        FilterTeamJobsRequestDto filter,
        CancellationToken ct = default)
    {
        var query = _teamJobRepository.Query()
            .AsNoTracking()
            .Include(j => j.Team)
            .Where(j => j.Status == TeamJobStatus.open)
            .ApplyFilters(filter)
            .ApplySearch(filter.Search)
            .ApplySorting(filter);

        var page = await PaginatedResult<TeamJob>.CreateAsync(query, filter.PageNumber, filter.PageSize, ct);

        var mapped = PaginatedResult<TeamJobDto>.FromList(
            page.Items.Select(x => x.ToDto()).ToList(),
            page.PageNumber,
            page.PageSize,
            page.TotalCount);

        return ApiResponse.Success(mapped);
    }

    public async Task<ApiResponse<TeamJobDetailsDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _teamJobRepository.GetByIdWithDetailsAsync(id, ct);

        if (job is null)
            return ApiResponse.Failure<TeamJobDetailsDto>(AppError.NotFound(nameof(TeamJob), id));

        return ApiResponse.Success(job.ToDetailsDto());
    }

    public async Task<ApiResponse<IEnumerable<TeamJobDto>>> GetByTeamIdAsync(Guid teamId, CancellationToken ct = default)
    {
        if (!await _teamRepository.ExistsAsync(teamId, ct))
            return ApiResponse.Failure<IEnumerable<TeamJobDto>>(AppError.NotFound(nameof(Team), teamId));

        var jobs = await _teamJobRepository.GetAllByTeamIdAsync(teamId, ct);
        return ApiResponse.Success(jobs.Select(x => x.ToDto()));
    }
}
