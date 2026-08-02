using FreeGency.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> CountUnreadAsync(
            Guid chatRoomId,
            Guid? clientProfileId,
            Guid? developerProfileId,
            CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .CountAsync(m =>
                    m.ChatRoomId == chatRoomId &&
                    !((clientProfileId != null && m.SenderClientProfileId == clientProfileId) ||
                      (developerProfileId != null && m.SenderDeveloperProfileId == developerProfileId)),
                    ct);

        public IQueryable<Message> GetByRoomIdAsync(Guid roomId)
            => _dbSet
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId);

        public async Task<IEnumerable<Message>> GetLatestAsync(Guid roomId, int count, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
    }
}
