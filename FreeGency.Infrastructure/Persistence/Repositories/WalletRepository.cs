
namespace FreeGency.Infrastructure.Persistence.Repositories;

public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context) : base(context) { }


    public async Task<Wallet?> GetByOwnerAsync(owner ownerType, Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.OwnerType == ownerType && w.OwnerId == ownerId, cancellationToken);
    }

    public async Task<Wallet?> GetByOwnerWithLedgerEntriesAsync(owner ownerType, Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(w => w.LedgerEntries)
            .FirstOrDefaultAsync(w => w.OwnerType == ownerType && w.OwnerId == ownerId, cancellationToken);
    }





}
