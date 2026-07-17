using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface ISpecialtyRepository : IGenericRepository<Specialty>
    {
        Task<IEnumerable<Specialty>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    }
}
