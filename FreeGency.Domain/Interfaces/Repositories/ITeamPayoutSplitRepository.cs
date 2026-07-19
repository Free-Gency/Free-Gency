using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{

    public interface ITeamPayoutSplitRepository : IGenericRepository<TeamPayoutSplit>
    {
        Task<IEnumerable<TeamPayoutSplit>> GetByTeamAndProjectAsync(Guid teamId,Guid? projectId,CancellationToken ct = default);

        Task ReplaceSplitsAsync(Guid teamId,Guid? projectId,IEnumerable<TeamPayoutSplit> splits,CancellationToken ct = default);

        Task<bool> ValidateSplitsAsync(IEnumerable<TeamPayoutSplit> splits, decimal totalAmount, CancellationToken ct = default);
    }
}
