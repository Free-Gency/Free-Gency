
namespace FreeGency.Infrastructure.Persistence.Repositories.Plan;

public class SubscriptionRepository: GenericRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }
}
