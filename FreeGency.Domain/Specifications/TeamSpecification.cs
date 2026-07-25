using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class TeamSpecification:BaseSpecification<Team>
    {
        public TeamSpecification(string code):base(x=>x.TeamCode==code)
        {
            
        }
    }
}
