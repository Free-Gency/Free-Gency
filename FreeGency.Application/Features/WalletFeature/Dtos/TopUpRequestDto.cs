using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.WalletFeature.Dtos
{
    public class TopUpRequestDto
    {
        [Required]
        public decimal Amount { get; set; }

    }
}
