
namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {

        Task<Project?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

        Task<IEnumerable<Project>> GetByClientIdAsync(Guid clientId, ProjectStatus? status = null, CancellationToken ct = default);

        Task<IEnumerable<Project>> SearchOpenAsync(string? keyword, Guid? categoryI, Guid? specialtyId, decimal? minBudget, decimal? maxBudget, CancellationToken ct = default);

        Task AddWithSkillsAsync(Project project, IEnumerable<Guid> skillIds, CancellationToken ct = default);

        Task UpdateStatusAsync(Guid id, ProjectStatus status, CancellationToken ct = default);

        /// <summary>In-progress projects whose milestones are all released (stuck completion).</summary>
        Task<IReadOnlyList<Guid>> GetInProgressIdsReadyToCompleteAsync(CancellationToken ct = default);

        Task SetAssigneeAsync(Guid id, Guid? userId, Guid? teamId, CancellationToken ct = default);

        Task ReplaceSkillsAsync(Guid projectId, IEnumerable<Guid> skillIds, CancellationToken ct = default);

        Task ReplaceSpecialtiesAsync(Guid projectId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default);


        Task SaveProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default);

        Task UnsaveProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default);

        Task<IEnumerable<Project>> GetSavedByUserAsync(Guid userId, CancellationToken ct = default);

        Task<IEnumerable<Project>> GetMineAsync(
            Guid userId,
            bool asClient,
            CancellationToken ct = default);

        IQueryable<Project> GetProjectsQuery();

        Task<int> CountCreatedByClientSinceAsync(Guid clientId, DateTime since, CancellationToken ct = default);
    }
}
