using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class EscrowHoldRepository : GenericRepository<EscrowHold>, IEscrowHoldRepository
    {
        public EscrowHoldRepository(ApplicationDbContext context):base(context)
        {
            
        }
        public async Task<EscrowHold?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(p=>p.ProjectId== projectId,ct);
            
        }

        public async Task RecordReleaseAsync(Guid projectId, decimal amount, CancellationToken ct = default)
        {
            if(amount<=0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            var escrow= await _dbSet.FirstOrDefaultAsync(p=>p.ProjectId == projectId,ct);
            if (escrow is null)
                throw new KeyNotFoundException("Escrow hold not found");
            if (escrow.TotalReleased+amount > escrow.TotalAmount)
                throw new InvalidOperationException("Released amount exceeds escrow total.");
            escrow.TotalReleased += amount;

            if (escrow.TotalReleased == escrow.TotalAmount)
            {
                escrow.FundingStatus = FundingStatus.Completed;
            }
            _dbSet.Update(escrow);
        }

        public async Task UpdateFundingStatusAsync(Guid projectId,FundingStatus status,CancellationToken ct = default)
        {
            var escrow = await _dbSet.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);

            if (escrow is null)
                throw new KeyNotFoundException("Escrow hold not found.");
            escrow.FundingStatus = status;
            if (status == FundingStatus.Locked)
            {
                escrow.LockedAt ??= DateTime.UtcNow;
            }
            _dbSet.Update(escrow);
        }
        public async Task UpdatePlanStatusAsync(Guid projectId, PlanStatus status, CancellationToken ct = default)
        {
            var escrow= await _dbSet.FirstOrDefaultAsync(p=>p.ProjectId== projectId, ct);
            if (escrow is null)
                throw new KeyNotFoundException("Escrow hold not found.");
            escrow.planStatus = status;
            if(status==PlanStatus.PlanAgreed)
            {
                escrow.PlanAgreedAt ??= DateTime.UtcNow;
            }
            _dbSet.Update(escrow);
        }
    }
}
