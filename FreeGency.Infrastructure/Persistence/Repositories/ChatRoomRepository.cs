using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ChatRoomRepository : GenericRepository<ChatRoom>, IChatRoomRepository
{
    public ChatRoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public IQueryable<ChatRoom> GetChatRoomQueryable(Guid? clientProfileId, Guid? developerProfileId)
    {
        return _dbSet.Where(x => x.ChatRoomMembers.Any(m =>
            (clientProfileId != null && m.ClientProfileId == clientProfileId) ||
            (developerProfileId != null && m.DeveloperProfileId == developerProfileId)));
    }

    public async Task<IEnumerable<ChatRoom>> GetByProfileAsync(
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(r => r.ChatRoomMembers.Any(m =>
                (clientProfileId != null && m.ClientProfileId == clientProfileId) ||
                (developerProfileId != null && m.DeveloperProfileId == developerProfileId)))
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ChatRoom?> GetTeamMainAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TeamId == teamId && r.RoomType == RoomType.TeamMain, ct);
    }

    public async Task<ChatRoom?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.RoomType == RoomType.Project, ct);
    }

    public async Task<ChatRoom?> GetByProposalIdAsync(Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProposalId == proposalId && r.RoomType == RoomType.Proposal, ct);
    }

    public async Task<ChatRoom?> GetByProposalIdForUpdateAsync(Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.ProposalId == proposalId, ct);
    }

    public async Task AddWithMembersAsync(
        ChatRoom room,
        IEnumerable<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)> members,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (room.Id == Guid.Empty)
            room.Id = Guid.NewGuid();

        await _dbSet.AddAsync(room, ct);

        var now = DateTime.UtcNow;
        var entities = members
            .Where(m => m.ClientProfileId.HasValue ^ m.DeveloperProfileId.HasValue)
            .GroupBy(m => m.ClientProfileId.HasValue
                ? ("C", m.ClientProfileId!.Value)
                : ("D", m.DeveloperProfileId!.Value))
            .Select(g => g.First())
            .Select(m => new ChatRoomMember
            {
                Id = Guid.NewGuid(),
                ChatRoomId = room.Id,
                ClientProfileId = m.ClientProfileId,
                DeveloperProfileId = m.DeveloperProfileId,
                JoinedAt = now,
                CanSend = m.CanSend,
                RoleLabel = m.RoleLabel
            });

        await _context.Set<ChatRoomMember>().AddRangeAsync(entities, ct);
    }

    public async Task AddMemberAsync(
        Guid roomId,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default,
        bool canSend = true,
        string? roleLabel = null)
    {
        if (clientProfileId.HasValue == developerProfileId.HasValue)
            throw new ArgumentException("Exactly one of clientProfileId or developerProfileId must be set.");

        var exists = await _context.Set<ChatRoomMember>()
            .AnyAsync(x =>
                x.ChatRoomId == roomId &&
                ((clientProfileId != null && x.ClientProfileId == clientProfileId) ||
                 (developerProfileId != null && x.DeveloperProfileId == developerProfileId)), ct);
        if (exists)
            return;

        await _context.Set<ChatRoomMember>().AddAsync(new ChatRoomMember
        {
            Id = Guid.NewGuid(),
            ChatRoomId = roomId,
            ClientProfileId = clientProfileId,
            DeveloperProfileId = developerProfileId,
            JoinedAt = DateTime.UtcNow,
            CanSend = canSend,
            RoleLabel = roleLabel
        }, ct);
    }

    public async Task RemoveMemberAsync(
        Guid roomId,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default)
    {
        var member = await FindMemberAsync(roomId, clientProfileId, developerProfileId, ct);
        if (member is null)
            return;
        _context.Set<ChatRoomMember>().Remove(member);
    }

    public async Task UpdateLastReadAsync(
        Guid roomId,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default)
    {
        var member = await FindMemberAsync(roomId, clientProfileId, developerProfileId, ct);
        if (member is null)
            throw new KeyNotFoundException("Chat room member not found.");
        member.LastReadAt = DateTime.UtcNow;
        _context.Set<ChatRoomMember>().Update(member);
    }

    public async Task<bool> RoomIsExist(Guid RoomId)
    {
        return await _dbSet.AnyAsync(x => x.Id == RoomId);
    }

    private async Task<ChatRoomMember?> FindMemberAsync(
        Guid roomId,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct)
    {
        return await _context.Set<ChatRoomMember>()
            .FirstOrDefaultAsync(x =>
                x.ChatRoomId == roomId &&
                ((clientProfileId != null && x.ClientProfileId == clientProfileId) ||
                 (developerProfileId != null && x.DeveloperProfileId == developerProfileId)), ct);
    }
}
