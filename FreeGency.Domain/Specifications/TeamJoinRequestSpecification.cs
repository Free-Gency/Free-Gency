using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class TeamJoinRequestSpecification:BaseSpecification<TeamJoinRequest>
    {
        public TeamJoinRequestSpecification(Guid teamId,Guid userId):base(
            x=>x.UserId==userId&&x.TeamId==teamId && x.Status== TeamJoinRequestStatus.pending)
        {
            
        }
        public TeamJoinRequestSpecification(Guid requestId):base(x=>x.Id==requestId)
        {
            Includes.Add(x => x.Team);
            Includes.Add(x => x.User);
        }
        public TeamJoinRequestSpecification(TeamJoinRequestSpecificationParam param):base(
            x =>
                    x.TeamId == param.TeamId &&
                (string.IsNullOrEmpty(param.Status) ||
                 x.Status.ToString().ToLower() == param.Status)
            )
        {
            ApplyPagination(
               (param.PageNumber - 1) * param.PageSize,
               param.PageSize);
            AddOrderByDescending(x => x.RequestedAt);
            Includes.Add(x => x.User);
            Includes.Add(x => x.TeamJob);
            Includes.Add(x => x.User.DeveloperProfile);
        }
        public TeamJoinRequestSpecification(Guid userId, UserJoinRequestSpecificationParam param)
        : base(x =>
            x.UserId == userId &&
            (param.Status == null || x.Status == param.Status))
        {
            ApplyPagination((param.PageNumber - 1) * param.PageSize, param.PageSize);
            AddOrderByDescending(x => x.RequestedAt);

            Includes.Add(x => x.Team);
            Includes.Add(x => x.TeamJob);
        }
    }
}
