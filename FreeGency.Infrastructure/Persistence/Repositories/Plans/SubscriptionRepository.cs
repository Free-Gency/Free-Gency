
namespace FreeGency.Infrastructure.Persistence.Repositories.Plans;

public class SubscriptionRepository: GenericRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }
}
