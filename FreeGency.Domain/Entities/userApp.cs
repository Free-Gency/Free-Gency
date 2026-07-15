using FreeGency.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class userApp :IdentityUser
    {
        public string FristName { get; set; }
        public string LastName { get; set; }
        public bool IsVerified { get; set; }
        public profileMode ActiveProfileMode { get; set; }
        
    }
}
