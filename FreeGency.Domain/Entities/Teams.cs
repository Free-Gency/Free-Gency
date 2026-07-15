using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class Teams
    {
        public Guid OwnerUserId { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string TeamCode { get; set; }
        public string AboutUs { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
