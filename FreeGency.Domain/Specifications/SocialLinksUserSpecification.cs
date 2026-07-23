using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class SocialLinksUserSpecification:BaseSpecification<SocialLink>
    {
        public SocialLinksUserSpecification(Guid userId):base(x=>x.OwnerUserId==userId)
        {
            
        }
    }
}
