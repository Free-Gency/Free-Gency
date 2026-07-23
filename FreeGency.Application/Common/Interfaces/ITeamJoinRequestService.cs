using FreeGency.Application.Features.TeamJoinRequests.Dtos;
using FreeGency.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Text;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace FreeGency.Application.Common.Interfaces
{
    public interface ITeamJoinRequestService
    {
        Task<Result> ApplyToTeamJobAsync(ApplyToTeamJobCommand applyToTeamJob);
        Task<Result> JoinByCodeAsync(JoinTeamByCodeCommand joinTeamByCode);
        Task<Result<PaginatedResult<TeamJoinRequestResponseDto>>> GetJoinRequestAsync(TeamJoinRequestSpecificationParam param);
        Task<Result<PaginatedResult<UserRequestJoinResponseDto>>> GetUserJoinRequestsAsync(UserJoinRequestSpecificationParam param);
        public  Task<Result> AcceptJoinRequestAsync(Guid requestId);
        public Task<Result> RejectJoinRequestAsync(Guid requestId);

    }
}
