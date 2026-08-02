using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        IQueryable<Message> GetByRoomIdAsync(Guid roomId);
        Task<IEnumerable<Message>> GetLatestAsync(Guid roomId, int count, CancellationToken ct = default);
        Task<int> CountUnreadAsync(Guid chatRoomId, Guid? clientProfileId, Guid? developerProfileId, CancellationToken ct = default);
    }
}
