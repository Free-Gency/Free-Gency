using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class ClientProfileRepository : GenericRepository<ClientProfile>, IClientProfileRepository
    {
        public ClientProfileRepository(ApplicationDbContext context) : base(context) { }


        public async Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .AnyAsync(cp => cp.UserId == userId, ct);

        public async Task<ClientProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(cp => cp.UserId == userId, ct);

        public async Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default)
            => await _dbSet
                .Where(cp => cp.UserId == userId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(cp => cp.AverageRating, avg)
                     .SetProperty(cp => cp.RatingCount, count), ct);
    }
}
