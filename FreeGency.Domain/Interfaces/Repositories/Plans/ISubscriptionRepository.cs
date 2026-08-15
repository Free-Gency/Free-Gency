
namespace FreeGency.Domain.Interfaces.Repositories.Plans;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    Task<List<Subscription>> GetExpireSubscription();
    
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<Subscription?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default);

}
