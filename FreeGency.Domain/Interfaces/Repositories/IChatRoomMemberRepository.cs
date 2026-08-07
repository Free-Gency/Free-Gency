using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IChatRoomMemberRepository : IGenericRepository<ChatRoomMember>
    {
        Task<ChatRoomMember?> IsMember(Guid? clientProfileId, Guid? developerProfileId, Guid roomId);
        Task<List<ChatRoomMember>> GetRoomProfileIdsAsync(Guid roomId);
        Task<IReadOnlyList<(Guid UserId, string Name, string? RoleLabel, bool CanSend)>> GetDeveloperMembersAsync(
            Guid roomId,
            CancellationToken ct = default);
    }
}
