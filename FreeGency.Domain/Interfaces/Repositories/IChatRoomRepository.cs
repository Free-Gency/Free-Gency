using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IChatRoomRepository : IGenericRepository<ChatRoom>
    {
        Task<IEnumerable<ChatRoom>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task<ChatRoom?> GetTeamMainAsync(Guid teamId,CancellationToken ct = default);

        Task<ChatRoom?> GetByProjectIdAsync(Guid projectId,CancellationToken ct = default);

        Task<ChatRoom?> GetByProposalIdAsync(Guid proposalId,CancellationToken ct = default);

        Task AddWithMembersAsync(ChatRoom room,IEnumerable<Guid> memberUserIds,CancellationToken ct = default);

        Task AddMemberAsync(Guid roomId,Guid userId,CancellationToken ct = default);

        Task RemoveMemberAsync(Guid roomId,Guid userId,CancellationToken ct = default);

        Task UpdateLastReadAsync(Guid roomId,Guid userId,CancellationToken ct = default);

        Task SetReadOnlyAsync(Guid roomId,CancellationToken ct = default);
    }
}
