using FreeGency.Domain.Interfaces.Repositories;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool? isRead = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task MarkAllReadAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task MarkReadAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
