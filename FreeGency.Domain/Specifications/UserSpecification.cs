using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class UserSpecification:BaseSpecification<User>
    {
        public UserSpecification(Guid userId):base(x=>x.Id==userId)
        {
            
        }
    }
}
