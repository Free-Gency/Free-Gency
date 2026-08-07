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
            var active = await GetActiveProfileAsync(userId);
            if (active is null)
                throw new InvalidOperationException("User has no active profile.");
            return active.Value.ProfileId;
        }

        public async Task<(Guid ProfileId, profileMode Mode)?> GetActiveProfileAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            var user = await _dbSet
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.ActiveProfileMode,
                    ClientProfileId = x.ClientProfile != null ? (Guid?)x.ClientProfile.Id : null,
                    DeveloperProfileId = x.DeveloperProfile != null ? (Guid?)x.DeveloperProfile.Id : null
                })
                .FirstOrDefaultAsync(ct);

            if (user is null || user.ActiveProfileMode is null)
                return null;

            if (user.ActiveProfileMode == profileMode.Client && user.ClientProfileId.HasValue)
                return (user.ClientProfileId.Value, profileMode.Client);

            if (user.ActiveProfileMode == profileMode.Developer && user.DeveloperProfileId.HasValue)
                return (user.DeveloperProfileId.Value, profileMode.Developer);

            return null;
        }

        public async Task<Guid?> GetClientProfileIdByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _context.Set<ClientProfile>()
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);

        public async Task<Guid?> GetDeveloperProfileIdByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _context.Set<DeveloperProfile>()
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);

        public async Task UpdateActiveProfileModeAsync(Guid userId, profileMode mode, CancellationToken ct = default)
            => await _dbSet
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ActiveProfileMode, mode), ct);
    }
}
