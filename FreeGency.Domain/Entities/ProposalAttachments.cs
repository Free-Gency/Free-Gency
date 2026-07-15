using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class ProposalAttachments
    {
        public Guid ProposalId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
}
