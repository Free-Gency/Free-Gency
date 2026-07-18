using FreeGency.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> CountUnreadAsync(Guid chatRoomId, Guid userId, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .CountAsync(m =>
                    m.ChatRoomId == chatRoomId &&
                    m.SenderUserId != userId,
                    ct);

        public IQueryable<Message> GetByRoomIdAsync(Guid roomId, int skip, int take)
            => _dbSet
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .Skip(skip)
                .Take(take);

        public async Task<IEnumerable<Message>> GetLatestAsync(Guid roomId, int count, CancellationToken ct = default)
            => await _dbSet
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
    }
}
