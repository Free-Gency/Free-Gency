
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IWalletRepository : IGenericRepository<Wallet>
{
    
    Task<Wallet?> GetByOwnerAsync(owner ownerType, Guid ownerId, CancellationToken cancellationToken = default);

    Task<Wallet?> GetByOwnerWithLedgerEntriesAsync(owner ownerType, Guid ownerId, CancellationToken cancellationToken = default);


}
