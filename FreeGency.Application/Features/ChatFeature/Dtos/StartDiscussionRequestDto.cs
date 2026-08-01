using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class StartDiscussionRequestDto
    {
        [Required]
        public Guid ProposalId { get; init; }
    }
}
