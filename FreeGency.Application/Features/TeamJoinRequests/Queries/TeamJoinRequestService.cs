using FreeGency.Application.Features.TeamJoinRequests.Dtos;
using FreeGency.Application.Features.TeamJoinRequests.Mapping;
using FreeGency.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Commands
{
    public partial class TeamJoinRequestService
    {
        public async Task<Result<PaginatedResult<TeamJoinRequestResponseDto>>> GetJoinRequestAsync(TeamJoinRequestSpecificationParam param)
        {
            var userId = currentUserService.UserId;
            var IsLeader = await _teamMemberRepository.IsLeaderAsync(param.TeamId, userId);
            if (!IsLeader) return Result.Failure<PaginatedResult<TeamJoinRequestResponseDto>>(TeamErrors.UnauthorizedLeader);
            var listSpec = new TeamJoinRequestSpecification(param);

            var requests = await _teamJoinRequestRepository.ListAsync(listSpec);

            var response = requests.Select(x => x.ToDto()).ToList();

            var totalCount = await _teamJoinRequestRepository.CountAsync(listSpec);

            var paginatedResult = PaginatedResult<TeamJoinRequestResponseDto>.FromList(
                response,
                param.PageNumber,
                param.PageSize,
                totalCount);

            return Result.Success(paginatedResult);
        }
        public async Task<Result<PaginatedResult<UserRequestJoinResponseDto>>> GetUserJoinRequestsAsync(UserJoinRequestSpecificationParam param)
        {
            var userId = currentUserService.UserId;

            var spec = new TeamJoinRequestSpecification(userId, param);

            var requests = await _teamJoinRequestRepository.ListAsync(spec);

            var response = requests.Select(x => x.ToUserRequestJoinDto()).ToList();

            var totalCount = await _teamJoinRequestRepository.CountAsync(spec);

            var paginatedResult = PaginatedResult<UserRequestJoinResponseDto>.FromList(
                response,
                param.PageNumber,
                param.PageSize,
                totalCount);

            return Result.Success(paginatedResult);
        }
    }
}
