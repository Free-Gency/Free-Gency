using FreeGency.Domain.Interfaces.Repositories;
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
            using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var existingInterests = _context.UserInterests
                    .Where(ui => ui.UserId == userId);

                _context.UserInterests.RemoveRange(existingInterests);

                if (categoryIds != null && categoryIds.Any())
                {
                    var newInterests = categoryIds.Select(categoryId => new UserInterest
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CategoryId = categoryId
                    });

                    await _context.UserInterests.AddRangeAsync(newInterests, ct);
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task ReplaceSkillsAsync(Guid userId, IEnumerable<Guid> skillIds, CancellationToken ct = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                await _context.UserSkills
                    .Where(us => us.UserId == userId)
                    .ExecuteDeleteAsync(ct);

                if (skillIds != null && skillIds.Any())
                {
                    var newSkills = skillIds.Select(skillId => new UserSkill
                    {
                        UserId = userId,
                        SkillId = skillId
                    });

                    await _context.UserSkills.AddRangeAsync(newSkills, ct);
                    await _context.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<IEnumerable<DeveloperProfile>> SearchBySkillsAsync(IEnumerable<Guid> skillIds, CancellationToken ct = default)
        {
            if (skillIds == null || !skillIds.Any())
            {
                return Enumerable.Empty<DeveloperProfile>();
            }

            var profiles = await _dbSet
                .AsNoTracking()
                .Where(dp => _context.UserSkills
                    .Any(us => us.UserId == dp.UserId && skillIds.Contains(us.SkillId)))
                .Include(dp => dp.User)
                    .ThenInclude(u => u.UserSkills)
                        .ThenInclude(us => us.Skill)
                .ToListAsync(ct);

            return profiles;
        }

        public async Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default)
            => await _dbSet
            .Where(dp => dp.UserId == userId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(dp => dp.AverageRating, avg)
                 .SetProperty(dp => dp.RatingCount, count),
            ct);
    }
}
