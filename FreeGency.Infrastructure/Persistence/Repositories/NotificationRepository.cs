namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> CountUnreadAsync(
            Guid? clientProfileId,
            Guid? developerProfileId,
            CancellationToken ct = default)
        {
            EnsureSingleProfile(clientProfileId, developerProfileId);

            return await _dbSet
                .AsNoTracking()
                .Where(n =>
                    ((clientProfileId != null && n.ClientProfileId == clientProfileId) ||
                     (developerProfileId != null && n.DeveloperProfileId == developerProfileId)) &&
                    !n.IsRead)
                .CountAsync(ct);
        }

        public async Task<IEnumerable<Notification>> GetByProfileAsync(
            Guid? clientProfileId,
            Guid? developerProfileId,
            bool? isRead = null,
            CancellationToken ct = default)
        {
            EnsureSingleProfile(clientProfileId, developerProfileId);

            var query = _dbSet.AsNoTracking().Where(n =>
                (clientProfileId != null && n.ClientProfileId == clientProfileId) ||
                (developerProfileId != null && n.DeveloperProfileId == developerProfileId));

            if (isRead != null)
            {
                query = isRead == true ? query.Where(n => n.IsRead) : query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task MarkAllReadAsync(
            Guid? clientProfileId,
            Guid? developerProfileId,
            CancellationToken ct = default)
        {
            EnsureSingleProfile(clientProfileId, developerProfileId);

            await _dbSet
                .Where(n =>
                    (clientProfileId != null && n.ClientProfileId == clientProfileId) ||
                    (developerProfileId != null && n.DeveloperProfileId == developerProfileId))
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
        }

        public async Task MarkReadAsync(Guid id, CancellationToken ct = default)
            => await _dbSet
                .Where(n => n.Id == id)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);

        private static void EnsureSingleProfile(Guid? clientProfileId, Guid? developerProfileId)
        {
            if (clientProfileId.HasValue == developerProfileId.HasValue)
                throw new ArgumentException("Exactly one of clientProfileId or developerProfileId must be set.");
        }

        public  IQueryable<Notification> GetNotificationAsync(Guid profilId,Guid userId)
        {
            return _dbSet.Where(x =>(x.UserId==userId)||
                                    (x.ClientProfileId!=null && x.ClientProfileId==profilId)||
                                    (x.DeveloperProfileId!=null&&x.DeveloperProfileId==profilId)
                                    );
        }

        public async Task<Notification?> GetUnreadChatNotificationAsync(Guid chatRoomId, Guid? clientProfileId, Guid? developerProfileId)
        {
            return await _dbSet.Where(x => !x.IsRead &&
                                        x.ChatRoomId == chatRoomId &&
                                        x.Type == NotificationType.NewChatMessage &&
                                        x.ClientProfileId == clientProfileId &&
                                        x.DeveloperProfileId == developerProfileId).FirstOrDefaultAsync();
        }
    }
}
