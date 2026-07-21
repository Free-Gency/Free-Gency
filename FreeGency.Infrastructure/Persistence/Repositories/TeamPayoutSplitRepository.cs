using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class TeamPayoutSplitRepository: GenericRepository<TeamPayoutSplit>, ITeamPayoutSplitRepository
{
    public TeamPayoutSplitRepository(ApplicationDbContext context): base(context)
    {
    }

    public async Task<IEnumerable<TeamPayoutSplit>> GetByTeamAndProjectAsync(Guid teamId,Guid? projectId,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Where(x => x.TeamId == teamId && x.ProjectId == projectId).OrderBy(x => x.CreatedAt).ToListAsync(ct);
    }

    public async Task ReplaceSplitsAsync(Guid teamId,Guid? projectId,IEnumerable<TeamPayoutSplit> splits,CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(splits);

        var oldSplits = await _dbSet.Where(x => x.TeamId == teamId && x.ProjectId == projectId).ToListAsync(ct);

        if (oldSplits.Any())
            _dbSet.RemoveRange(oldSplits);
        var newSplits = splits.ToList();
        foreach (var split in newSplits)
        {
            if (split.Id == Guid.Empty)
                split.Id = Guid.NewGuid();
            split.TeamId = teamId;
            split.ProjectId = projectId;
        }

        if (newSplits.Any())
            await _dbSet.AddRangeAsync(newSplits, ct);
    }

    public Task<bool> ValidateSplitsAsync(IEnumerable<TeamPayoutSplit> splits,decimal totalAmount,CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(splits);

        var list = splits.ToList();

        if (!list.Any())
            return Task.FromResult(false);

        var splitType = list.First().SplitType;

        if (list.Any(x => x.SplitType != splitType))
            return Task.FromResult(false);

        if (splitType == SplitType.Percent)
        {
            return Task.FromResult(list.Sum(x => x.Value) == 100m);
        }

        if (totalAmount <= 0)
            return Task.FromResult(false);

        if (list.Any(x => x.Value <= 0))
            return Task.FromResult(false);

        return Task.FromResult(list.Sum(x => x.Value) == totalAmount);
    }
}