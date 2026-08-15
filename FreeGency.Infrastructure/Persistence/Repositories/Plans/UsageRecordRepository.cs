
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class UsageRecordRepository: GenericRepository<UsageRecord>, IUsageRecordRepository
{
    public UsageRecordRepository(ApplicationDbContext context) : base(context)
    {
    }
}
