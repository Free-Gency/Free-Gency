using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Entities
{
    public class Wallet
    {
        public owner OwnerType {  get; set; }
        public Guid OwnerId { get; set; }
        public string Currency {  get; set; }
        public decimal Available {  get; set; }
        public decimal Reserved { get; set; }
        public decimal Pending { get; set; }
    }
}
