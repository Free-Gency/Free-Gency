using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IChatRoomRepository : IGenericRepository<ChatRoom>
{
    Task<bool> RoomIsExist(Guid RoomId);
    IQueryable<ChatRoom> GetChatRoomQueryable(Guid? clientProfileId, Guid? developerProfileId);
    Task<IEnumerable<ChatRoom>> GetByProfileAsync(Guid? clientProfileId, Guid? developerProfileId, CancellationToken ct = default);

    Task<ChatRoom?> GetTeamMainAsync(Guid teamId, CancellationToken ct = default);

    Task<ChatRoom?> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Tracked project room for status updates (archive on completion, etc.).</summary>
    Task<ChatRoom?> GetByProjectIdForUpdateAsync(Guid projectId, CancellationToken ct = default);

    Task<ChatRoom?> GetByProposalIdAsync(Guid proposalId, CancellationToken ct = default);

    /// <summary>Tracked entity for status updates (archive, etc.).</summary>
    Task<ChatRoom?> GetByProposalIdForUpdateAsync(Guid proposalId, CancellationToken ct = default);

    Task AddWithMembersAsync(
        ChatRoom room,
        IEnumerable<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)> members,
        CancellationToken ct = default);

    Task AddMemberAsync(
        Guid roomId,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default,
        bool canSend = true,
        string? roleLabel = null);

    Task RemoveMemberAsync(Guid roomId, Guid? clientProfileId, Guid? developerProfileId, CancellationToken ct = default);

    Task UpdateLastReadAsync(Guid roomId, Guid? clientProfileId, Guid? developerProfileId, CancellationToken ct = default);
}
