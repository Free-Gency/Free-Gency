using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IMilestoneRepository : IGenericRepository<Milestone>
    {
        Task<IEnumerable<Milestone>> GetByProjectIdAsync(Guid projectId,CancellationToken ct = default);
        Task UpdateWorkStatusAsync(Guid milestoneId,WorkStatus status,CancellationToken ct = default);
        Task UpdateReleaseStatusAsync(Guid milestoneId,ReleaseStatus status,CancellationToken ct = default);
        Task<decimal> SumAmountsByProjectAsync(Guid projectId,CancellationToken ct = default);
        Task<bool> AllReleasedAsync(Guid projectId,CancellationToken ct = default);
    }
}
    
