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

        public async Task<Guid> GetProfileId(Guid userId)
        {
            var user = await _dbSet.Where(x => x.Id == userId).Include(x => x.ClientProfile).Include(x => x.DeveloperProfile).FirstOrDefaultAsync();
            if (user.ActiveProfileMode == profileMode.Client) return user.ClientProfile!.Id;
            else return user.DeveloperProfile!.Id;
        }

        public async Task UpdateActiveProfileModeAsync(Guid userId, profileMode mode, CancellationToken ct = default)
            => await _dbSet
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ActiveProfileMode, mode), ct);
    }
}
