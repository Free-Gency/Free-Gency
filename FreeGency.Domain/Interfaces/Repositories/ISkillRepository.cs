using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface ISkillRepository : IGenericRepository<Skill>
    {
        Task<IEnumerable<Skill>> SearchAsync(string query, int limit, CancellationToken ct = default);
        Task<IEnumerable<Skill>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
