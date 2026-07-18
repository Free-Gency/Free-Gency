using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface ISocialLinkRepository : IGenericRepository<SocialLink>
    {
        Task<IEnumerable<SocialLink>> GetByOwnerAsync(owner type, Guid ownerId, CancellationToken ct = default);
    }
}
