using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.WalletFeature.Dtos
{
    public class WalletUserDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Currency { get; set; } 
        public decimal Available { get; set; }
        public decimal Reserved { get; set; }
        public decimal Pending { get; set; }
    }
}
