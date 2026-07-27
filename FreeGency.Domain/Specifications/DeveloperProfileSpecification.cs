using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class DeveloperProfileSpecification:BaseSpecification<DeveloperProfile>
    {
        public DeveloperProfileSpecification(Guid userId):base(x=>x.UserId==userId)
        {
            
        }
    }
}
