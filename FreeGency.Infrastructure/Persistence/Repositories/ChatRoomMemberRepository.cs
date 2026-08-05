using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class ChatRoomMemberRepository : GenericRepository<ChatRoomMember>, IChatRoomMemberRepository
    {
        public ChatRoomMemberRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Guid>> GetRoomProfileIdsAsync(Guid roomId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.ChatRoomId == roomId)
                .Where(x => x.ClientProfileId != null || x.DeveloperProfileId != null)
                .Select(x => x.ClientProfileId ?? x.DeveloperProfileId!.Value)
                .ToListAsync();
        }

        public async Task<ChatRoomMember?> IsMember(Guid? clientProfileId, Guid? developerProfileId, Guid roomId)
        {
            return await _dbSet.FirstOrDefaultAsync(x =>
                x.ChatRoomId == roomId &&
                ((clientProfileId != null && x.ClientProfileId == clientProfileId) ||
                 (developerProfileId != null && x.DeveloperProfileId == developerProfileId)));
        }

        public async Task<IReadOnlyList<(Guid UserId, string Name, string? RoleLabel, bool CanSend)>> GetDeveloperMembersAsync(
            Guid roomId,
            CancellationToken ct = default)
        {
            var rows = await _dbSet.AsNoTracking()
                .Where(x => x.ChatRoomId == roomId && x.DeveloperProfileId != null)
                .Select(x => new
                {
                    UserId = x.DeveloperProfile!.UserId,
                    FirstName = x.DeveloperProfile.User.FristName,
                    LastName = x.DeveloperProfile.User.LastName,
                    x.RoleLabel,
                    x.CanSend
                })
                .ToListAsync(ct);

            return rows
                .Select(x => (
                    x.UserId,
                    $"{x.FirstName} {x.LastName}".Trim(),
                    x.RoleLabel,
                    x.CanSend))
                .ToList();
        }
    }
}
