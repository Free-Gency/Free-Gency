
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class SubscriptionRepository: GenericRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Subscription>> GetExpireSubscription()
    {
        return await _dbSet.Include(x=>x.Plan).Where(x => x.ExpiresAt <= DateTime.UtcNow).ToListAsync();
    }
}
