using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IClientProfileRepository : IGenericRepository<ClientProfile>
{
    Task<string> GetEmail(Guid ProfileId);
    Task<ClientProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default);
    Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default);
    Task AddInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default);
    Task ReplaceInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default);
    Task AddSpecialtiesAsync(Guid userId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default);
    Task ReplaceSpecialtiesAsync(Guid userId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default);
    Task ReplaceSkillsAsync(Guid userId, IEnumerable<Guid> skillIds, CancellationToken ct = default);
}
