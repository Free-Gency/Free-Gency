
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;


public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(s => s.UserId == userId
                     && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing)
                     && (s.ExpiresAt == null || s.ExpiresAt > now))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Subscription?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);
}