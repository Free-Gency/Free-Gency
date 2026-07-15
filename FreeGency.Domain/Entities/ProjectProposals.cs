using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ProjectProposals
    {
        public Guid ProjectId { get; set; }
        public ApplicantType ApplicantType { get; set; }
        public Guid TeamId { get; set; }
        public string UserId { get; set; }
        public string CoverLetter { get; set; }
        public decimal ProposedBudget { get; set; }
        public ProposalStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime ResponseAt { get; set; }
    }
}
