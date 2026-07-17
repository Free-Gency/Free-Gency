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
    }
}
