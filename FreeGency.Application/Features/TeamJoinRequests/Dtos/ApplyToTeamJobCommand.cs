using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Dtos
{
    public class ApplyToTeamJobCommand
    {
        [Required]
        public Guid JobId { get; set; }
        public string? CoverLetter {get; set;}
    }
}
