using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class PaymentTransactionRepository:GenericRepository<PaymentTransaction>,IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(ApplicationDbContext context):base(context)
        {
            
        }
    }
}
