using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class Projects
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ClientId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid SpecialtyId { get; set; }
        public bool IsFixedPrice { get; set; }
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public string Currency {  get; set; }
        public DateTime Deadline { get; set; }
        public int EstimatedDurationDays { get; set; }
        public ProjectStatus Status { get; set; }
        public Guid AssignedTeamId { get; set; }
        public string AssignedUserId { get; set; }

        public DateTime CompletedAt { get; set; }

    }
}
