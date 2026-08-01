using AutoMapper.Execution;
using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.ChatFeature.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Commands
{
    public partial class ChatService
    {
        public async Task<Result<PaginatedResult<ChatRoomDto>>> GetChatRoomAsync(ChatRoomFilter filter)
        {
            var userId = currentUserService.UserId;
            var query = _chatRoomRepository.GetChatRoomQueryable(userId);

            if (filter.RoomType.HasValue)
            {
                query = query.Where(x => x.RoomType == filter.RoomType.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x =>
                    x.Title!.Contains(filter.Search));
            }

            var result = await PaginatedResult<ChatRoomDto>.CreateAsync(
                query
                    .ToChatRoomListDto(userId)
                    .OrderByDescending(x => x.LastMessageAt),
                filter.PageNumber,
                filter.PageSize
                );
            return Result.Success(result);
        }
        public async Task<Result<PaginatedResult<RoomMessagesDto>>> GetMessageChatRoom(Guid ChatRoomId, RoomMessageFilter pagedQuery)
        {
            var room = await _chatRoomRepository.RoomIsExist(ChatRoomId);
            if (!room) return Result.Failure<PaginatedResult<RoomMessagesDto>>(ChatErrors.ChatRoomNotFound);
            var userId = currentUserService.UserId;
            var member = await _chatRoomMemberRepository.IsMember(userId, ChatRoomId);
            if (member == null) return Result.Failure<PaginatedResult<RoomMessagesDto>>(ChatErrors.UserNotMember);
            var Messages = _messageRepository.GetByRoomIdAsync(ChatRoomId);
            var result = Messages.ToRoomMessageDto(userId);
            var pagination = await PaginatedResult<RoomMessagesDto>.CreateAsync(result, pagedQuery.PageNumber, pagedQuery.PageSize);
            member.LastReadAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
            return Result.Success(pagination);
        }
    }
}
