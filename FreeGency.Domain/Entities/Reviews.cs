using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class Reviews
    {
        public Guid ProjectId { get; set; }
        public Guid ReviewerUserId { get; set; }
        public RevieweeType RevieweeType { get; set; }
        public Guid RevieweeTeamId { get; set; }
        public Guid RevieweeUserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

    }
}
