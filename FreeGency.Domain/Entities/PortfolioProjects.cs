using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class PortfolioProjects
    {
        public owner OwnerType { get; set; }
        public Guid OwnerUserId { get; set; }
        public Guid OwnerTeamId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Budget {  get; set; }
        public string ImageCover { get; set; }
        public string ProjectUrl { get; set; }
        public DateTime CompletionDate { get; set; }
        public Guid CategoryId { get; set; }
        public Visibility Visibility { get; set; }
    }
}
