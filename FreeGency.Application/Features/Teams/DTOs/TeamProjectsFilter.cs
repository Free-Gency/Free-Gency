using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.Teams.DTOs
{
    public class TeamProjectsFilter:PagedQuery
    {
        [Required]
        public Guid TeamId { get; init;}
    }
}
