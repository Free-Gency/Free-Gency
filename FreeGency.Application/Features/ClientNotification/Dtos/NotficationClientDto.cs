using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ClientNotification.Dtos
{
    public class NotficationClientDto
    {
        public Guid Id { get; set; }
        public bool NewMessageInApp { get; set; }
        public bool NewMessageEmail { get; set; }
        public bool ProposalReceivedInApp { get; set; }
        public bool ProposalReceivedEmail { get; set; }

        public bool MilestoneAddedInApp { get; set; }
        public bool MilestoneAddedEmail { get; set; }

        public bool WalletUpdatedInApp { get; set; }
        public bool WalletUpdatedEmail { get; set; }
    }
}
