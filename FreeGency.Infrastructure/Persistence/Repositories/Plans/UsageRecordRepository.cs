
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;


public class UsageRecordRepository : GenericRepository<UsageRecord>, IUsageRecordRepository
{
    public UsageRecordRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UsageRecord?> GetBySubscriptionAndFeatureAsync(Guid subscriptionId, FeatureType feature, CancellationToken ct = default)
        => await _dbSet
            .Where(r => r.SubscriptionId == subscriptionId && r.Feature == feature)
            .FirstOrDefaultAsync(ct);

}