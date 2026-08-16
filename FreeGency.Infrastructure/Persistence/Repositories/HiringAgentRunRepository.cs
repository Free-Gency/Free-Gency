using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class HiringAgentRunRepository : GenericRepository<HiringAgentRun>, IHiringAgentRunRepository
{
    public HiringAgentRunRepository(ApplicationDbContext context) : base(context)
    {
    }

    private static readonly HiringAgentRunStatus[] ActiveStatuses =
    [
        HiringAgentRunStatus.Queued,
        HiringAgentRunStatus.Inviting,
        HiringAgentRunStatus.WaitingAccepts,
        HiringAgentRunStatus.Discussing,
        HiringAgentRunStatus.Ranking
    ];

    public async Task<HiringAgentRun?> GetByIdWithCandidatesAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(r => r.Candidates)
            .Include(r => r.Project)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<HiringAgentRun?> GetActiveByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(r => r.Candidates)
            .Where(r => r.ProjectId == projectId && ActiveStatuses.Contains(r.Status))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> HasActiveRunForProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(
            r => r.ProjectId == projectId && ActiveStatuses.Contains(r.Status),
            ct);
    }

    public async Task<IReadOnlyList<HiringAgentRun>> GetByClientAsync(
        Guid clientUserId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(r => r.Candidates)
            .Include(r => r.Project)
            .Where(r => r.ClientUserId == clientUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<HiringAgentRun?> GetByInvitationIdAsync(Guid invitationId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(r => r.Candidates)
            .Where(r => r.Candidates.Any(c => c.InvitationId == invitationId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<HiringAgentCandidate?> GetCandidateByChatRoomIdAsync(
        Guid chatRoomId,
        CancellationToken ct = default)
    {
        return await _context.Set<HiringAgentCandidate>()
            .Include(c => c.HiringAgentRun)
            .FirstOrDefaultAsync(c => c.ChatRoomId == chatRoomId, ct);
    }

    public async Task<HiringAgentRun?> GetByCandidateIdWithDetailsAsync(
        Guid candidateId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(r => r.Candidates)
            .Include(r => r.Project)
            .FirstOrDefaultAsync(r => r.Candidates.Any(c => c.Id == candidateId), ct);
    }

    public async Task<HiringAgentRun?> GetClientApprovedReportReadyByProposalIdAsync(
        Guid proposalId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(r => r.Candidates)
            .Include(r => r.Project)
            .Where(r =>
                r.Status == HiringAgentRunStatus.ReportReady
                && r.ClientHireApprovedAt != null
                && r.RecommendedProposalId == proposalId)
            .OrderByDescending(r => r.ClientHireApprovedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task PersistMatchInviteResultsAsync(
        Guid runId,
        IReadOnlyList<HiringAgentCandidate> candidates,
        CancellationToken ct = default)
    {
        // Force a clean tracker: navigation.Add() with client Guids can incorrectly mark rows Modified.
        _context.ChangeTracker.Clear();

        foreach (var candidate in candidates)
        {
            candidate.HiringAgentRunId = runId;
            _context.Entry(candidate).State = EntityState.Added;
        }

        var run = new HiringAgentRun { Id = runId };
        _context.HiringAgentRuns.Attach(run);
        var hasDiscussing = candidates.Any(c =>
            c.Status is HiringAgentCandidateStatus.Discussing
                or HiringAgentCandidateStatus.PlanProposed
                or HiringAgentCandidateStatus.Accepted);
        run.Status = hasDiscussing
            ? HiringAgentRunStatus.Discussing
            : HiringAgentRunStatus.WaitingAccepts;
        run.UpdatedAt = DateTime.UtcNow;
        _context.Entry(run).Property(r => r.Status).IsModified = true;
        _context.Entry(run).Property(r => r.UpdatedAt).IsModified = true;

        await _context.SaveChangesAsync(ct);
    }
}
