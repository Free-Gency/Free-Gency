using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IDeveloperProfileRepository : IGenericRepository<DeveloperProfile>
    {
        Task<DeveloperProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<DeveloperProfile?> GetByUserIdWithSkillsAndInterestsAsync(Guid userId, CancellationToken ct = default);
        Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default);
        Task ReplaceSkillsAsync(Guid userId, IEnumerable<Guid> skillIds, CancellationToken ct = default);
        Task ReplaceInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default);
        Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default);
        Task<IEnumerable<DeveloperProfile>> SearchBySkillsAsync(IEnumerable<Guid> skillIds, CancellationToken ct = default);
    }
}
