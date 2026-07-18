

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class LedgerEntryRepository : GenericRepository<LedgerEntry>, ILedgerEntryRepository
{
    public LedgerEntryRepository(ApplicationDbContext context) : base(context) { }



    public async Task<IReadOnlyList<LedgerEntry>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(le => le.WalletId == walletId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(le => le.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

}
