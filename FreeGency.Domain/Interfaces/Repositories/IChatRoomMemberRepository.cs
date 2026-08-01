using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IChatRoomMemberRepository:IGenericRepository<ChatRoomMember>
    {
        Task<ChatRoomMember?> IsMember(Guid userId, Guid roomId);
    }
}
