using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Dtos
{
    public class UserRequestJoinResponseDto
    {
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? TeamLogo { get; set; }

        public Guid? TeamJobId { get; set; }
        public string? TeamJobTitle { get; set; }

        public string? CoverLetter { get; set; }

        public TeamJoinRequestStatus Status { get; set; }

        public DateTime RequestedAt { get; set; }
    }
}
