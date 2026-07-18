using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetWithSpecialtiesAsync(Guid id, CancellationToken ct = default);
    }
}
