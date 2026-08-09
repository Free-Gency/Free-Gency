using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.WalletFeature.Dtos
{
    public class WalletTeamDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public string Currency { get; set; }
        public decimal Available { get; set; }
        public decimal Reserved { get; set; }
        public decimal Pending { get; set; }
        public decimal TotalEarnings { get; set; }
    }
}
