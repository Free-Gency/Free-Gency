using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.DeveloperNotification.Dtos
{
    public class NotficationDeveloperDto
    {
        public Guid Id { get; set; }

        public bool MessagesInApp { get; set; } = true;
        public bool MessagesEmail { get; set; } = true;

        public bool ProjectsInApp { get; set; } = true;
        public bool ProjectsEmail { get; set; } = false;

        public bool MilestonesInApp { get; set; } = true;
        public bool MilestonesEmail { get; set; } = true;

        public bool WalletInApp { get; set; } = true;
        public bool WalletEmail { get; set; } = true;

        public bool TeamsInApp { get; set; } = true;
        public bool TeamsEmail { get; set; } = false;
    }
}
