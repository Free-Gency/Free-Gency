using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class MilestoneRepository:GenericRepository<Milestone>,IMilestoneRepository
{
    public MilestoneRepository(ApplicationDbContext context):base(context)
    {
    }
    public async Task<bool> AreAllOtherMilestonesApprovedAsync(
     Guid projectId,
     Guid currentMilestoneId,
     CancellationToken ct = default)
    {
        return !await _dbSet.AnyAsync(
            m => m.ProjectId == projectId &&
                 m.Id != currentMilestoneId &&
                 m.WorkStatus != WorkStatus.Approved,
            ct);
    }
    public async Task<IEnumerable<Milestone>> GetByProjectIdAsync(Guid projectId,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(m => m.ProjectFiles)
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<Milestone?> GetNextUnfundedAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(m => m.ProjectId == projectId && !m.IsFunded && m.ReleaseStatus != ReleaseStatus.Released)
            .OrderBy(m => m.SortOrder)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Milestone>> GetDueForAutoReleaseAsync(DateTime cutoffUtc, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(m =>
                m.WorkStatus == WorkStatus.Submitted &&
                m.ReleaseStatus == ReleaseStatus.Pending &&
                m.AvailableAt != null &&
                m.AvailableAt <= cutoffUtc)
            .ToListAsync(ct);
    }

    public async Task UpdateWorkStatusAsync(Guid milestoneId,WorkStatus status,CancellationToken ct = default)
    {
        var milestone = await _dbSet.FirstOrDefaultAsync(m => m.Id == milestoneId, ct);
        if (milestone is null)
            throw new KeyNotFoundException("Milestone not found.");
        milestone.WorkStatus = status;
        if (status == WorkStatus.Submitted)
            milestone.SubmittedAt = DateTime.UtcNow;
        _dbSet.Update(milestone);
    }
    public async Task UpdateReleaseStatusAsync(Guid milestoneId,ReleaseStatus status,CancellationToken ct = default)
    {
        var milestone = await _dbSet.FirstOrDefaultAsync(m => m.Id == milestoneId, ct);
        if (milestone is null)
            throw new KeyNotFoundException("Milestone not found.");
        milestone.ReleaseStatus = status;
        switch (status)
        {
            case ReleaseStatus.Pending:
                milestone.AvailableAt ??= DateTime.UtcNow;
                break;
            case ReleaseStatus.Released:
                milestone.ReleasedAt ??= DateTime.UtcNow;
                milestone.ReleasedAmount = milestone.Amount;
                break;
        }
        _dbSet.Update(milestone);
    }
    public async Task<decimal> SumAmountsByProjectAsync(Guid projectId,CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().Where(m => m.ProjectId == projectId).SumAsync(m => m.Amount, ct);
    }
    public async Task<bool> AllReleasedAsync(Guid projectId,CancellationToken ct = default)
    {
        var milestones = _dbSet.AsNoTracking().Where(m => m.ProjectId == projectId);
        if (!await milestones.AnyAsync(ct))
            return false;
        return await milestones.AllAsync(m => m.ReleaseStatus == ReleaseStatus.Released,ct);
    }
}