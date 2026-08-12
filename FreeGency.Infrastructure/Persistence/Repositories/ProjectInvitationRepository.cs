using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ProjectInvitationRepository
    : GenericRepository<ProjectInvitation>, IProjectInvitationRepository
{
    public ProjectInvitationRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<ProjectInvitation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(i => i.Project)
            .Include(i => i.ClientUser)
            .Include(i => i.InviteeUser)
            .Include(i => i.InviteeTeam)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<bool> HasPendingAsync(
        Guid projectId,
        ApplicantType inviteeType,
        Guid inviteeId,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(i => i.ProjectId == projectId && i.Status == ProjectInvitationStatus.Pending);

        query = inviteeType switch
        {
            ApplicantType.User => query.Where(i => i.InviteeUserId == inviteeId),
            ApplicantType.Team => query.Where(i => i.InviteeTeamId == inviteeId),
            _ => throw new ArgumentOutOfRangeException(nameof(inviteeType))
        };

        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectInvitation>> GetSentByClientAsync(
        Guid clientUserId,
        ProjectInvitationStatus? status,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.InviteeUser)
            .Include(i => i.InviteeTeam)
            .Where(i => i.ClientUserId == clientUserId);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectInvitation>> GetReceivedForDeveloperAsync(
        Guid developerUserId,
        IReadOnlyList<Guid> leaderTeamIds,
        ProjectInvitationStatus? status,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.ClientUser)
            .Include(i => i.InviteeTeam)
            .Where(i =>
                (i.InviteeType == ApplicantType.User && i.InviteeUserId == developerUserId)
                || (i.InviteeType == ApplicantType.Team
                    && i.InviteeTeamId.HasValue
                    && leaderTeamIds.Contains(i.InviteeTeamId.Value)));

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectInvitation>> GetForTeamAsync(
        Guid teamId,
        ProjectInvitationStatus? status,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.ClientUser)
            .Where(i => i.InviteeType == ApplicantType.Team && i.InviteeTeamId == teamId);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectInvitation>> GetPendingByProjectIdAsync(
        Guid projectId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(i => i.ProjectId == projectId && i.Status == ProjectInvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }
}
