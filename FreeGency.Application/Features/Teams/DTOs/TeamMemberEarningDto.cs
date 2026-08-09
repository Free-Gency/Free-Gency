using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Teams.DTOs
{
    public class TeamMemberEarningDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public decimal ReleasedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
