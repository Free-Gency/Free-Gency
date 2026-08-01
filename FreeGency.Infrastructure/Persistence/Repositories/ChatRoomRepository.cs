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
    public IQueryable<ChatRoom> GetChatRoomQueryable(Guid userId)
    {
        return _dbSet.Where(x => x.ChatRoomMembers.Any(m => m.UserId == userId));
    }

    public async Task<IEnumerable<ChatRoom>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(r => r.ChatRoomMembers.Any(m => m.UserId == userId))
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
            .FirstOrDefaultAsync(r => r.ProposalId == proposalId && r.RoomType==RoomType.Proposal, ct);
    }

    public async Task<ChatRoom?> GetByProposalIdForUpdateAsync(Guid proposalId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.ProposalId == proposalId, ct);
    }

    public Task AddWithMembersAsync(ChatRoom room, IEnumerable<Guid> memberUserIds, CancellationToken ct = default)
        => AddWithMembersAsync(
            room,
            memberUserIds.Select(id => (id, true, (string?)null)),
            ct);

    public async Task AddWithMembersAsync(
        ChatRoom room,
        IEnumerable<(Guid UserId, bool CanSend, string? RoleLabel)> members,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (room.Id == Guid.Empty)
            room.Id = Guid.NewGuid();

        await _dbSet.AddAsync(room, ct);

        var now = DateTime.UtcNow;
        var entities = members
            .GroupBy(m => m.UserId)
            .Select(g => g.First())
            .Select(m => new ChatRoomMember
            {
                Id = Guid.NewGuid(),
                ChatRoomId = room.Id,
                UserId = m.UserId,
                JoinedAt = now,
                CanSend = m.CanSend,
                RoleLabel = m.RoleLabel
            });

        await _context.Set<ChatRoomMember>().AddRangeAsync(entities, ct);
    }

    public async Task AddMemberAsync(
        Guid roomId,
        Guid userId,
        CancellationToken ct = default,
        bool canSend = true,
        string? roleLabel = null)
    {
        var exists = await _context.Set<ChatRoomMember>()
            .AnyAsync(x => x.ChatRoomId == roomId && x.UserId == userId, ct);
        if (exists)
            return;

        await _context.Set<ChatRoomMember>().AddAsync(new ChatRoomMember
        {
            Id = Guid.NewGuid(),
            ChatRoomId = roomId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            CanSend = canSend,
            RoleLabel = roleLabel
        }, ct);
    }

    public async Task RemoveMemberAsync(Guid roomId, Guid userId, CancellationToken ct = default)
    {
        var member = await _context.Set<ChatRoomMember>()
            .FirstOrDefaultAsync(x => x.ChatRoomId == roomId && x.UserId == userId, ct);
        if (member is null)
            return;
        _context.Set<ChatRoomMember>().Remove(member);
    }

    public async Task UpdateLastReadAsync(Guid roomId, Guid userId, CancellationToken ct = default)
    {
        var member = await _context.Set<ChatRoomMember>()
            .FirstOrDefaultAsync(x => x.ChatRoomId == roomId && x.UserId == userId, ct);
        if (member is null)
            throw new KeyNotFoundException("Chat room member not found.");
        member.LastReadAt = DateTime.UtcNow;
        _context.Set<ChatRoomMember>().Update(member);
    }

    public async Task<bool> RoomIsExist(Guid RoomId)
    {
        return await _dbSet.AnyAsync(x => x.Id == RoomId);
    }
}
