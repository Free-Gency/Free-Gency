using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class SocialLinks
    {
        public owner OwnerType { get; set; }
        public Guid OwnerUserId { get; set; }
        public Guid OwnerTeamId { get; set; }
        public string Platform { get; set; }
        public string Url { get; set; }
    }
}
