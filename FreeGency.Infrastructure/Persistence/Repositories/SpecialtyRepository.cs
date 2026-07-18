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
            => await _dbSet
                .AsNoTracking()
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync(ct);
    }
}
