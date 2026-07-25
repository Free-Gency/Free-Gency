

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class LedgerEntryRepository : GenericRepository<LedgerEntry>, ILedgerEntryRepository
{
    public LedgerEntryRepository(ApplicationDbContext context) : base(context) { }



    public async Task<IReadOnlyList<LedgerEntry>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(le => le.WalletId == walletId)
            .OrderByDescending(le => le.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(le => le.ProjectId == projectId)
            .OrderByDescending(le => le.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(le => le.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<LedgerEntry?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(le => le.IdempotencyKey == idempotencyKey, cancellationToken);
    }

}
