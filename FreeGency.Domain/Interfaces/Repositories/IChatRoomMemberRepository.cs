using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IChatRoomMemberRepository : IGenericRepository<ChatRoomMember>
    {
        Task<ChatRoomMember?> IsMember(Guid? clientProfileId, Guid? developerProfileId, Guid roomId);
        Task<List<Guid>> GetRoomProfileIdsAsync(Guid roomId);
    }
}
