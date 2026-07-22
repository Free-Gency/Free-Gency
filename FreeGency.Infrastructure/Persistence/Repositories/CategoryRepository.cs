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
        {
            var category = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (category is null)
                return null;

            var specialties = await _context.Set<CategorySpecialty>()
                .AsNoTracking()
                .Where(cs => cs.CategoryId == id)
                .Select(cs => cs.Specialty)
                .ToListAsync(ct);

            category.Specialties = specialties;
            return category;
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(c => c.Name == name || c.NameEn == name);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }
    }
}
