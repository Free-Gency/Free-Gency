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

    [Obsolete("Use GetByScopeAsync with milestoneId: null")]
    Task<IEnumerable<TeamPayoutSplit>> GetByTeamAndProjectAsync(
        Guid teamId,
        Guid? projectId,
        CancellationToken ct = default);
}
