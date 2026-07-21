using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool? isRead = null, CancellationToken ct = default);
        Task MarkReadAsync(Guid id, CancellationToken ct = default);
        Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
        Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);
    }
}
