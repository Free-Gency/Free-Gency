using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Category?> GetWithSpecialtiesAsync(Guid id, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .Include(c => c.CategorySpecialties)
                    .ThenInclude(cs => cs.Specialty)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(c => c.Name == name || c.NameEn == name);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }
    }
}
