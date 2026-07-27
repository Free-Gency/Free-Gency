using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class paymentTransactionSpecification:BaseSpecification<PaymentTransaction>
    {
        public paymentTransactionSpecification(string paymentId):base(x=>x.PaymentProviderRef==paymentId)
        {
            
        }
    }
}
