using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.Email == email, ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task UpdateActiveProfileModeAsync(Guid userId, profileMode mode, CancellationToken ct = default)
            => await _dbSet
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ActiveProfileMode, mode), ct);
    }
}
