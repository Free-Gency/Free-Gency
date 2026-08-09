

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class LedgerEntryRepository : GenericRepository<LedgerEntry>, ILedgerEntryRepository
{
    public LedgerEntryRepository(ApplicationDbContext context) : base(context) { }

    public Task<decimal> GetTotalEarningsAsync(Guid walletId)
    {
        return _context.LedgerEntries
            .Where(x =>
                x.WalletId == walletId &&
                x.EntryType == EntryType.TeamSplit)
            .SumAsync(x => x.Amount);
    }
    public IQueryable<LedgerEntry> GetByWalletId(Guid walletId)
    {
        return _dbSet
            .AsNoTracking()
            .Where(x => x.WalletId == walletId)
            .OrderByDescending(x => x.CreatedAt);
    }
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
