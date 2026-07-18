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
                .Include(c => c.Specialties)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
