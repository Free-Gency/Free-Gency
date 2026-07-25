using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
    {
        public SpecialtyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Specialty>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
            => await _context.Set<CategorySpecialty>()
                .AsNoTracking()
                .Where(cs => cs.CategoryId == categoryId)
                .Select(cs => cs.Specialty)
                .ToListAsync(ct);
        
        public async Task<IEnumerable<Skill>> GetSkillsForSpecialtyAsync(Guid specialtyId, CancellationToken ct = default)
        {
            return await _context.Set<SpecialtySkill>()
                .AsNoTracking()
                .Where(ss => ss.SpecialtyId == specialtyId)
                .Select(ss => ss.Skill)
                .ToListAsync(ct);
        }
    }
}
