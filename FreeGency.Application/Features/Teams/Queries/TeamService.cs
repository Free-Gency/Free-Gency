using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Mappings.TeamsMapping;
using FreeGency.Application.Common.Pagination;
using FreeGency.Application.Features.Teams.Dtos;

namespace FreeGency.Application.Features.Teams.Commands
{
    public partial class TeamService
    {
        public async Task<ApiResponse<PaginatedResult<TeamDto>>> BrowseAsync(
            FilterTeamsRequestDto filter,
            CancellationToken ct = default)
        {
            var userId = _currentUserService.UserId;
            var (items, totalCount) = await _teamRepository.GetBrowseHubItemsPagedAsync(
                userId == Guid.Empty ? null : userId,
                filter.Search,
                filter.CategoryId,
                filter.ExcludeMine,
                filter.PageNumber,
                filter.PageSize,
                ct);

            var page = PaginatedResult<TeamDto>.FromList(
                items.Select(t => t.ToDto()).ToList(),
                filter.PageNumber,
                filter.PageSize,
                totalCount);

            return ApiResponse.Success(page);
        }

        public async Task<ApiResponse<TeamDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByIdWithDetailsAsync(id, ct);
            if (team is null)
                return ApiResponse.Failure<TeamDto>(AppError.NotFound(nameof(Team), id));

            var userId = _currentUserService.UserId;
            return ApiResponse.Success(team.ToDto(userId == Guid.Empty ? null : userId));
        }

        public async Task<ApiResponse<IEnumerable<TeamDto>>> GetMineAsync(CancellationToken ct = default)
        {
            var teams = await _teamRepository.GetMyHubItemsAsync(_currentUserService.UserId, ct);
            return ApiResponse.Success(teams.Select(t => t.ToDto()));
        }

        public async Task<ApiResponse<TeamDto>> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByTeamCodeWithDetailsAsync(teamCode, ct);
            if (team is null)
                return ApiResponse.Failure<TeamDto>(AppError.NotFound(nameof(Team), teamCode));

            var userId = _currentUserService.UserId;
            return ApiResponse.Success(team.ToDto(userId == Guid.Empty ? null : userId));
        }

        public async Task<ApiResponse<IReadOnlyList<TeamReviewDto>>> GetReviewsAsync(
            Guid teamId,
            CancellationToken ct = default)
        {
            if (!await _teamRepository.ExistsAsync(teamId, ct))
                return ApiResponse.Failure<IReadOnlyList<TeamReviewDto>>(AppError.NotFound(nameof(Team), teamId));

            var feedback = await _teamRepository.GetFeedbackAsync(teamId, 40, ct);
            return ApiResponse.Success<IReadOnlyList<TeamReviewDto>>(feedback.Select(MapTeamReview).ToList());
        }
    }
}
