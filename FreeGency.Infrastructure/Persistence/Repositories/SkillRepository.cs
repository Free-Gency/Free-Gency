using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class SkillRepository : GenericRepository<Skill>, ISkillRepository
    {
        public SkillRepository(ApplicationDbContext context) : base(context) { }


        public async Task<IEnumerable<Skill>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .Where(s => ids.Contains(s.Id))
                .ToListAsync(ct);

        public async Task<IEnumerable<Skill>> SearchAsync(string query, int limit, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .Where(s => s.Name.Contains(query))
                .OrderBy(s => s.Name)
                .Take(limit)
                .ToListAsync(ct);

        public async Task<IEnumerable<Skill>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken ct = default)
            => await _context.Set<SpecialtySkill>()
                .AsNoTracking()
                .Where(ss => ss.SpecialtyId == specialtyId)
                .Select(ss => ss.Skill)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);

        public async Task<IEnumerable<Skill>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
            => await _context.Set<CategorySpecialty>()
                .AsNoTracking()
                .Where(cs => cs.CategoryId == categoryId)
                .Join(
                    _context.Set<SpecialtySkill>().AsNoTracking(),
                    cs => cs.SpecialtyId,
                    ss => ss.SpecialtyId,
                    (_, ss) => ss.Skill)
                .Distinct()
                .OrderBy(s => s.Name)
                .ToListAsync(ct);

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(s => s.Name == name);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }
    }
}
