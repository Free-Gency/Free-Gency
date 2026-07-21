namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync(ct);

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, bool? isRead = null, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(n => n.UserId == userId);

            if (isRead != null)
            {
                query = isRead == true ? query.Where(n => n.IsRead) : query.Where(n => !n.IsRead);
            }

            return await query.ToListAsync(ct);
        }

        public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
            => await _dbSet
                .Where(n => n.UserId == userId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, DateTime.Now), ct);

        public async Task MarkReadAsync(Guid id, CancellationToken ct = default)
            => await _dbSet
                .Where(n => n.Id == id)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, DateTime.Now), ct);
    }
}
