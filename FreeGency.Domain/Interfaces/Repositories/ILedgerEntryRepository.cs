
using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface ILedgerEntryRepository : IGenericRepository<LedgerEntry>
{

    Task<IReadOnlyList<LedgerEntry>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LedgerEntry>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task<LedgerEntry?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

}
