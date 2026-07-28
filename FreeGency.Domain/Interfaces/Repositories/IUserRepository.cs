using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task UpdateActiveProfileModeAsync(Guid userId, profileMode mode, CancellationToken ct = default);
        Task<Guid> GetProfileId(Guid userId);
    }
}
