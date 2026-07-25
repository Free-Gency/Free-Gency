using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Mappings.TeamsMapping;
using FreeGency.Application.Features.Teams.Dtos;

namespace FreeGency.Application.Features.Teams.Commands
{
    public partial class TeamService
    {
        public async Task<ApiResponse<IEnumerable<TeamDto>>> BrowseAsync(CancellationToken ct = default)
        {
            var teams = await _teamRepository.GetAllAsync(ct);
            return ApiResponse.Success(teams.Select(t => t.ToDto()));
        }

        public async Task<ApiResponse<TeamDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByIdWithDetailsAsync(id, ct);
            if (team is null)
                return ApiResponse.Failure<TeamDto>(AppError.NotFound(nameof(Team), id));

            return ApiResponse.Success(team.ToDto());
        }

        public async Task<ApiResponse<IEnumerable<TeamDto>>> GetMineAsync(CancellationToken ct = default)
        {
            var teams = await _teamRepository.GetByOwnerUserIdWithDetailsAsync(_currentUserService.UserId, ct);
            return ApiResponse.Success(teams.Select(t => t.ToDto()));
        }

        public async Task<ApiResponse<TeamDto>> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByTeamCodeWithDetailsAsync(teamCode, ct);
            if (team is null)
                return ApiResponse.Failure<TeamDto>(AppError.NotFound(nameof(Team), teamCode));

            return ApiResponse.Success(team.ToDto());
        }
    }
}