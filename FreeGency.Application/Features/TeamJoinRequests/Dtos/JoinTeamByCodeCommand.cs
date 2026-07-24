using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Dtos
{
    public class JoinTeamByCodeCommand
    {
        [Required]
        public string code { get; set; }
        public string? CoverLetter { get; set; }
    }
}
