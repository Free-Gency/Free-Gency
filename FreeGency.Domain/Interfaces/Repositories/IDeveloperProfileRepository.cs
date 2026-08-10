using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IDeveloperProfileRepository : IGenericRepository<DeveloperProfile>
    {
        Task<string> GetEmail(Guid profileId);
        Task<DeveloperProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<DeveloperProfile?> GetByUserIdWithSkillsAndInterestsAsync(Guid userId, CancellationToken ct = default);
        Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default);
        Task ReplaceSkillsAsync(Guid userId, IEnumerable<Guid> skillIds, CancellationToken ct = default);
        Task AddInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default);
        Task ReplaceInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default);
        Task AddSpecialtiesAsync(Guid userId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default);
        Task ReplaceSpecialtiesAsync(Guid userId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default);
        Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default);
        Task<IEnumerable<DeveloperProfile>> SearchBySkillsAsync(IEnumerable<Guid> skillIds, CancellationToken ct = default);

        Task<IReadOnlyList<DeveloperFeedback>> GetFeedbackAsync(Guid developerUserId, int take, CancellationToken ct = default);
        Task<bool> HasFeedbackAsync(Guid developerUserId, Guid reviewerUserId, CancellationToken ct = default);
        Task AddFeedbackAsync(DeveloperFeedback feedback, CancellationToken ct = default);
    }
}
