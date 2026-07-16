using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class UserInterests
    {
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
