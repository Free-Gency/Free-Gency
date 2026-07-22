using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface ISkillRepository : IGenericRepository<Skill>
    {
        Task<IEnumerable<Skill>> SearchAsync(string query, int limit, CancellationToken ct = default);
        Task<IEnumerable<Skill>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task<IEnumerable<Skill>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken ct = default);
        Task<IEnumerable<Skill>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    }
}
