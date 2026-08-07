using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.DeveloperNotification.Dtos
{
    public class UpdateNotificationDeveloperDto
    {
        public Guid Id { get; set; }

        public bool MessagesInApp { get; set; } 
        public bool MessagesEmail { get; set; } 

        public bool ProjectsInApp { get; set; } 
        public bool ProjectsEmail { get; set; } 

        public bool MilestonesInApp { get; set; } 
        public bool MilestonesEmail { get; set; } 

        public bool WalletInApp { get; set; } 
        public bool WalletEmail { get; set; } 

        public bool TeamsInApp { get; set; } 
        public bool TeamsEmail { get; set; } 
    }
}
