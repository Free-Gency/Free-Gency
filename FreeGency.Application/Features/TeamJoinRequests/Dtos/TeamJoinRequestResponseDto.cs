using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Dtos
{
    public class TeamJoinRequestResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? ProfilePicture { get; set; }

        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int CompletedProjects { get; set; }

        public string? CoverLetter { get; set; }
        public TeamJoinRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }

        public Guid? TeamJobId { get; set; }
        public string? TeamJobTitle { get; set; }
    }
}
