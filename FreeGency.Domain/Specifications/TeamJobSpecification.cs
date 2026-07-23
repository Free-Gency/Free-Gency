using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class TeamJobSpecification:BaseSpecification<TeamJob>
    {
        public TeamJobSpecification(Guid jobId):base(
            x=>x.Id==jobId && x.Status==TeamJobStatus.open
            )
        {
        }
    }
}
