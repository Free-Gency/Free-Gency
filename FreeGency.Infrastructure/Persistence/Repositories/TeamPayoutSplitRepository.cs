using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class TeamPayoutSplitRepository : GenericRepository<TeamPayoutSplit>, ITeamPayoutSplitRepository
{
    public TeamPayoutSplitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TeamPayoutSplit>> GetByScopeAsync(
        Guid teamId,
        Guid? projectId,
        Guid? milestoneId,
        CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(x => x.TeamId == teamId
                        && x.ProjectId == projectId
                        && x.MilestoneId == milestoneId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TeamPayoutSplit>> GetByProjectAsync(
        Guid teamId,
        Guid projectId,
        CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(u => u!.DeveloperProfile)
            .Where(x => x.TeamId == teamId && x.ProjectId == projectId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<IEnumerable<TeamPayoutSplit>> GetByTeamAndProjectAsync(
        Guid teamId,
        Guid? projectId,
        CancellationToken ct = default)
        => GetByScopeAsync(teamId, projectId, milestoneId: null, ct);

    public async Task ReplaceSplitsAsync(
        Guid teamId,
        Guid? projectId,
        Guid? milestoneId,
        IEnumerable<TeamPayoutSplit> splits,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(splits);

        var oldSplits = await _dbSet
            .Where(x => x.TeamId == teamId
                        && x.ProjectId == projectId
                        && x.MilestoneId == milestoneId)
            .ToListAsync(ct);

        if (oldSplits.Count > 0)
            _dbSet.RemoveRange(oldSplits);

        var newSplits = splits.ToList();
        foreach (var split in newSplits)
        {
            if (split.Id == Guid.Empty)
                split.Id = Guid.NewGuid();
            split.TeamId = teamId;
            split.ProjectId = projectId;
            split.MilestoneId = milestoneId;
        }

        if (newSplits.Count > 0)
            await _dbSet.AddRangeAsync(newSplits, ct);
    }

    public Task<bool> ValidateSplitsAsync(
        IEnumerable<TeamPayoutSplit> splits,
        decimal totalAmount,
        CancellationToken ct = default)
        => ValidateSplitsAsync(splits, totalAmount, allowPartialPercent: false, ct);

    public Task<bool> ValidateSplitsAsync(
        IEnumerable<TeamPayoutSplit> splits,
        decimal totalAmount,
        bool allowPartialPercent,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(splits);

        var list = splits.ToList();
        if (list.Count == 0)
            return Task.FromResult(false);

        var splitType = list.First().SplitType;
        if (list.Any(x => x.SplitType != splitType))
            return Task.FromResult(false);

        if (splitType == SplitType.Percent)
        {
            var sum = list.Sum(x => x.Value);
            // Percent may be partial: remainder stays on the team wallet at release.
            // allowPartialPercent=false still requires an exact 100 for legacy team/project defaults.
            if (allowPartialPercent)
                return Task.FromResult(sum > 0 && sum <= 100m);
            return Task.FromResult(sum == 100m);
        }

        if (totalAmount <= 0)
            return Task.FromResult(false);

        if (list.Any(x => x.Value <= 0))
            return Task.FromResult(false);

        return Task.FromResult(list.Sum(x => x.Value) == totalAmount);
    }
}
