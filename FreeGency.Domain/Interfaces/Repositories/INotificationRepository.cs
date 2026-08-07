using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<Notification?> GetUnreadChatNotificationAsync(
                                                        Guid chatRoomId,
                                                        Guid? clientProfileId,
                                                        Guid? developerProfileId);
        IQueryable<Notification> GetNotificationAsync(Guid profilId,Guid userId);
        Task<IEnumerable<Notification>> GetByProfileAsync(
            Guid? clientProfileId,
            Guid? developerProfileId,
            bool? isRead = null,
            CancellationToken ct = default);

        Task MarkReadAsync(Guid id, CancellationToken ct = default);

        Task MarkAllReadAsync(
            Guid? clientProfileId,
            Guid? developerProfileId,
            CancellationToken ct = default);

        Task<int> CountUnreadAsync(
            Guid? clientProfileId,
            Guid? developerProfileId,
            CancellationToken ct = default);
    }
}
