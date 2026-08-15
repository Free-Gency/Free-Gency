
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class UsageRecordRepository: GenericRepository<UsageRecord>, IUsageRecordRepository
{
    public UsageRecordRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<UsageRecord>> GetUsageRecordsBySubId(Guid subId)
    {
        return await _dbSet.Where(x => x.SubscriptionId == subId).ToListAsync();
    }
    public async Task<List<UsageRecord>> GetActiveProposalUsagesAsync()
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Where(x =>
                x.Feature == FeatureType.SendProposal &&
                x.subscription.ExpiresAt > now)
            .ToListAsync();
    }
}
