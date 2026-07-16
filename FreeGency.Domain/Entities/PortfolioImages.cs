using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class PortfolioImages
    {
        public Guid PortfolioProjectId { get; set; }
        public string ImageUrl { get; set; }
        public int SortOrder { get; set; }
    }
}
