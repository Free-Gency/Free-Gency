using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class ClientAccountSpecifiaction:BaseSpecification<ClientProfile>
    {
        public ClientAccountSpecifiaction(Guid userId):base(x=>x.UserId==userId)
        {
            AddInclude("User");
        }
    }
}
