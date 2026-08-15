
namespace FreeGency.Domain.Interfaces.Repositories.Plans;

public interface IUsageRecordRepository : IGenericRepository<UsageRecord>
{
    Task<UsageRecord?> GetBySubscriptionAndFeatureAsync(Guid subscriptionId, FeatureType feature, CancellationToken ct = default);
}