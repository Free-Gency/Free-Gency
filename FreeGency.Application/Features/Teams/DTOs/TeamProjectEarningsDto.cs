using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Teams.DTOs
{
    public class TeamProjectEarningsDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty; 
        public decimal TotalBudget { get; set; } 
        public decimal ReleasedAmount { get; set; }
        public List<TeamMemberEarningDto> Members { get; set; } = [];
    }
}
