using FreeGency.Application.Features.ChatFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IChatService
    {
        Task<Result<Guid>> StartDiscussionAsync(StartDiscussionRequestDto dto);
        Task<Result<PaginatedResult<RoomMessagesDto>>> GetMessageChatRoom(Guid ChatRoomId, RoomMessageFilter pagedQuery);
        Task<Result<RoomMessagesDto>> SendMessageAsync(Guid ChatRoomId, SendMessageRequest sendMessageRequest);
        Task ApplyChatModerationAsync(
            Guid messageId,
            Guid userId,
            Guid? clientProfileId,
            Guid? developerProfileId,
            Guid senderProfileId,
            string senderProfileType,
            string senderName);
        Task<Result<PaginatedResult<ChatRoomDto>>> GetChatRoomAsync(ChatRoomFilter filter);
        Task<Result> MarkAsRead(Guid roomId);
        Task<Result> ArchiveRoomAsync(Guid roomId);
    }
}
