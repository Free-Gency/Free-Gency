
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Domain.Interfaces.Repositories.Plans;

public interface IUsageRecordRepository : IGenericRepository<UsageRecord>
{
    Task<List<UsageRecord>> GetUsageRecordsBySubId(Guid subId);
    Task<List<UsageRecord>> GetActiveProposalUsagesAsync();

    Task<UsageRecord?> GetBySubscriptionAndFeatureAsync(Guid subscriptionId, FeatureType feature, CancellationToken ct = default);
}
