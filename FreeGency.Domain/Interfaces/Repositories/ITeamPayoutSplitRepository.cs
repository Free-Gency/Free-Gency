using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface ITeamPayoutSplitRepository : IGenericRepository<TeamPayoutSplit>
{
    /// <summary>
    /// When <paramref name="milestoneId"/> is null, returns only team/project-level rows
    /// (<c>MilestoneId == null</c>). When set, returns that milestone's rows only.
    /// </summary>
    Task<IEnumerable<TeamPayoutSplit>> GetByScopeAsync(
        Guid teamId,
        Guid? projectId,
        Guid? milestoneId,
        CancellationToken ct = default);

    /// <summary>All splits for a project (project-level + every milestone).</summary>
    Task<IEnumerable<TeamPayoutSplit>> GetByProjectAsync(
        Guid teamId,
        Guid projectId,
        CancellationToken ct = default);

        Task ReplaceSplitsAsync(
            Guid teamId,
            Guid? projectId,
            Guid? milestoneId,
            IEnumerable<TeamPayoutSplit> splits,
            CancellationToken ct = default);

        Task<bool> ValidateSplitsAsync(
            IEnumerable<TeamPayoutSplit> splits,
            decimal totalAmount,
            CancellationToken ct = default);

        /// <param name="allowPartialPercent">
        /// When true, percent splits may sum to any value in (0, 100]; remainder goes to the team wallet on release.
        /// </param>
        Task<bool> ValidateSplitsAsync(
            IEnumerable<TeamPayoutSplit> splits,
            decimal totalAmount,
            bool allowPartialPercent,
            CancellationToken ct = default);

        [Obsolete("Use GetByScopeAsync with milestoneId: null")]
        Task<IEnumerable<TeamPayoutSplit>> GetByTeamAndProjectAsync(
            Guid teamId,
            Guid? projectId,
            CancellationToken ct = default);
}
