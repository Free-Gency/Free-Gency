using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Mappings.TeamsMapping;
using FreeGency.Application.Common.Pagination;
using FreeGency.Application.Features.Teams.Dtos;
using FreeGency.Application.Features.Teams.DTOs;
using FreeGency.Application.Features.WalletFeature.Dtos;

namespace FreeGency.Application.Features.Teams.Commands
{
    public partial class TeamService
    {
        public async Task<Result<WalletTeamDto>> GetTeamWallet(Guid teamid)
        { 
            var wallet = await _walletRepository.GetByOwnerAsync(owner.Team, teamid);
            if (wallet == null) return Result.Failure<WalletTeamDto>(WalletErrors.NotFound);
            var WalletTeamDto = new WalletTeamDto
            {
                Id = wallet.Id,
                Currency = wallet.Currency,
                Pending = wallet.Pending,
                Available = wallet.Available,
                Reserved = wallet.Reserved,
                TeamId=wallet.OwnerTeamId.Value
            };
            return Result.Success(WalletTeamDto);
        }
        public async Task<Result<PaginatedResult<TeamProjectEarningsDto>>>
      GetTeamProjectEarnings(TeamProjectsFilter teamProjectsFilter)
        {
            var teamProjects = _teamRepository
                .GetProjectTeamAccepted(teamProjectsFilter.TeamId);

            var query = teamProjects.Select(project => new TeamProjectEarningsDto
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                Currency = project.Currency,

                TotalBudget = project.MilestonePlanVersions
                    .Where(v => v.Status == PlanVersionStatus.Accepted)
                    .SelectMany(v => v.Items)
                    .Sum(x => x.Amount),

                ReleasedAmount = project.Milestones
                    .Sum(x => x.ReleasedAmount),

                Members = project.TeamPayoutSplits
                    .Select(split => new TeamMemberEarningDto
                    {
                        UserId = split.UserId,

                        Name = split.User.FristName + " " + split.User.LastName,

                        Percentage = split.Value,

                        Amount =
                            project.MilestonePlanVersions
                                .Where(v => v.Status == PlanVersionStatus.Accepted)
                                .SelectMany(v => v.Items)
                                .Sum(x => x.Amount)
                            * split.Value / 100,

                        ReleasedAmount =
                            project.Milestones
                                .Sum(x => x.ReleasedAmount)
                            * split.Value / 100,

                        Status =
                            project.Milestones.Sum(x => x.ReleasedAmount) == 0
                                ? "Pending"
                                : project.Milestones.Sum(x => x.ReleasedAmount)
                                    >= project.MilestonePlanVersions
                                        .Where(v => v.Status == PlanVersionStatus.Accepted)
                                        .SelectMany(v => v.Items)
                                        .Sum(x => x.Amount)
                                    ? "Released"
                                    : "Partially Released"
                    })
                    .ToList()
            });

            var result = await PaginatedResult<TeamProjectEarningsDto>.CreateAsync(
                query,
                teamProjectsFilter.PageNumber,
                teamProjectsFilter.PageSize);

            return Result.Success(result);
        }
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
