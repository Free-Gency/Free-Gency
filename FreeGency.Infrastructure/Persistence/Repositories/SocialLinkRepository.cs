using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public sealed class SocialLinkRepository : GenericRepository<SocialLink>, ISocialLinkRepository
    {
        public SocialLinkRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<SocialLink>> GetByOwnerAsync(
            owner type,
            Guid ownerId,
            CancellationToken ct = default
        )
            => await _dbSet
                .Where(sl => sl.OwnerType == type && sl.OwnerUserId == ownerId)
                .ToListAsync(ct);
    }
}
