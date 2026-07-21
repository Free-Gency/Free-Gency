using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Dtos
{
    public class ClientAccountResponseDto
    {
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Country { get; set; }

        public string? ProfileImage { get; set; }

        public string? Bio { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }

        public int ProjectsPostedCount { get; set; }

        public int ProjectsCompletedCount { get; set; }

        public decimal TotalSpent { get; set; }

        public DateTime JoinedAt { get; set; }
        public bool IsVerified { get; set; }
        public string? ProfileMode { get; set; }
    }
}
