
namespace FreeGency.Infrastructure.Persistence.Repositories;

public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context) : base(context) { }


    public async Task<Wallet?> GetByOwnerAsync(owner ownerType, Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(MatchesOwner(ownerType, ownerId), cancellationToken);
    }

    public async Task<Wallet?> GetByOwnerWithLedgerEntriesAsync(owner ownerType, Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(w => w.LedgerEntries)
            .FirstOrDefaultAsync(MatchesOwner(ownerType, ownerId), cancellationToken);
    }

    private static System.Linq.Expressions.Expression<Func<Wallet, bool>> MatchesOwner(owner ownerType, Guid ownerId)
    {
        return ownerType switch
        {
            owner.User => w => w.OwnerType == owner.User && w.OwnerUserId == ownerId,
            owner.Team => w => w.OwnerType == owner.Team && w.OwnerTeamId == ownerId,
            _ => w => false,
        };
    }





}
