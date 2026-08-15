using FreeGency.Application.Features.TeamJoinRequests.Dtos;
using FreeGency.Domain.Specifications;


namespace FreeGency.Application.Common.Interfaces;

public interface ITeamJoinRequestService
{
    Task<Result> ApplyToTeamJobAsync(ApplyToTeamJobCommand applyToTeamJob, CancellationToken ct= default);
    Task<Result> JoinByCodeAsync(JoinTeamByCodeCommand joinTeamByCode, CancellationToken ct= default);
    Task<Result<PaginatedResult<TeamJoinRequestResponseDto>>> GetJoinRequestAsync(TeamJoinRequestSpecificationParam param, CancellationToken ct = default);
    Task<Result<PaginatedResult<UserRequestJoinResponseDto>>> GetUserJoinRequestsAsync(UserJoinRequestSpecificationParam param, CancellationToken ct = default);
    public  Task<Result> AcceptJoinRequestAsync(Guid requestId, CancellationToken ct = default);
    public Task<Result> RejectJoinRequestAsync(Guid requestId, CancellationToken ct = default);

}
