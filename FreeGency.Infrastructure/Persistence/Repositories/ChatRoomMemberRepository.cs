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

        public async Task<ChatRoomMember?> IsMember(Guid? clientProfileId, Guid? developerProfileId, Guid roomId)
        {
            return await _dbSet.FirstOrDefaultAsync(x =>
                x.ChatRoomId == roomId &&
                ((clientProfileId != null && x.ClientProfileId == clientProfileId) ||
                 (developerProfileId != null && x.DeveloperProfileId == developerProfileId)));
        }
    }
}
