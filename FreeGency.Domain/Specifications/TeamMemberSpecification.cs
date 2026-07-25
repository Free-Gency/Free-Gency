using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class TeamMemberSpecification:BaseSpecification<TeamMember>
    {
        public TeamMemberSpecification(Guid TeamId,Guid UserId):base(x=>x.TeamId==TeamId && x.UserId==UserId)
        {
            
        }
    }
}
