using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IEscrowHoldRepository : IGenericRepository<EscrowHold>
    {
        Task<EscrowHold?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

        Task UpdateFundingStatusAsync(Guid projectId,FundingStatus status,CancellationToken ct = default);

        Task UpdatePlanStatusAsync(Guid projectId,PlanStatus status,CancellationToken ct = default);

        Task RecordReleaseAsync(Guid projectId,decimal amount,CancellationToken ct = default);
    }
}
