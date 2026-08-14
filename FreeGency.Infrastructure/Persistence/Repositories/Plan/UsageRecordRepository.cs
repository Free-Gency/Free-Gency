
namespace FreeGency.Infrastructure.Persistence.Repositories.Plan;

public class UsageRecordRepository: GenericRepository<UsageRecord>, IUsageRecordRepository
{
    public UsageRecordRepository(ApplicationDbContext context) : base(context)
    {
    }
}
