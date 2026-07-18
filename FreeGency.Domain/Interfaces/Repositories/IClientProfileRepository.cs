using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IClientProfileRepository : IGenericRepository<ClientProfile>
    {
        Task<ClientProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default);
        Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default);
    }
}
