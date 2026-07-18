using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class DeveloperProfileRepository : GenericRepository<DeveloperProfile>, IDeveloperProfileRepository
    {
        public DeveloperProfileRepository(ApplicationDbContext context) : base(context) { }


        public async Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .AnyAsync(dp => dp.UserId == userId, ct);

        public async Task<DeveloperProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(dp => dp.UserId == userId, ct);

        public async Task<DeveloperProfile?> GetByUserIdWithSkillsAndInterestsAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .AsSplitQuery()
                .Include(dp => dp.User)
                    .ThenInclude(u => u.UserSkills)
                        .ThenInclude(us => us.Skill)
                .Include(dp => dp.User)
                    .ThenInclude(u => u.UserInterests)
                        .ThenInclude(ui => ui.Category)
                .SingleOrDefaultAsync(dp => dp.UserId == userId, ct);

        public async Task ReplaceInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default)
        {

        }

        public Task ReplaceSkillsAsync(Guid userId, IEnumerable<Guid> skillIds, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DeveloperProfile>> SearchBySkillsAsync(IEnumerable<Guid> skillIds, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRatingAsync(Guid userId, decimal avg, int count)
        {
            throw new NotImplementedException();
        }
    }
}
