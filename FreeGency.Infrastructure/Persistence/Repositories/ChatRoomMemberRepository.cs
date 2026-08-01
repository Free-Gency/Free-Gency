using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class ChatRoomMemberRepository:GenericRepository<ChatRoomMember>, IChatRoomMemberRepository
    {
        public ChatRoomMemberRepository(ApplicationDbContext context):base(context)
        {
            
        }

        public async Task<ChatRoomMember?> IsMember(Guid userId, Guid roomId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.UserId == userId && x.ChatRoomId == roomId);
        }
    }
}
