using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class MilestonePlanItemRepository:GenericRepository<MilestonePlanItem>, IMilestonePlanItemRepository
    {
        public MilestonePlanItemRepository(ApplicationDbContext context):base(context)
        {
            
        }
    }
}
