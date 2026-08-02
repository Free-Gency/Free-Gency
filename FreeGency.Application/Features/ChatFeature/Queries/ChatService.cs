using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.ChatFeature.Mapping;

namespace FreeGency.Application.Features.ChatFeature.Commands
{
    public partial class ChatService
    {
        public async Task<Result<PaginatedResult<ChatRoomDto>>> GetChatRoomAsync(ChatRoomFilter filter)
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active is null) return Result.Failure<PaginatedResult<ChatRoomDto>>(ChatErrors.ActiveProfileRequired);

            var (clientProfileId, developerProfileId) = SplitActiveProfile(active.Value);
            var query = _chatRoomRepository.GetChatRoomQueryable(clientProfileId, developerProfileId);

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
                    .ToChatRoomListDto(clientProfileId, developerProfileId)
                    .OrderByDescending(x => x.LastMessageAt),
                filter.PageNumber,
                filter.PageSize
                );
            return Result.Success(result);
        }

        public async Task<Result<PaginatedResult<RoomMessagesDto>>> GetMessageChatRoom(
            Guid ChatRoomId,
            RoomMessageFilter pagedQuery)
        {
            var room = await _chatRoomRepository.RoomIsExist(ChatRoomId);
            if (!room) return Result.Failure<PaginatedResult<RoomMessagesDto>>(ChatErrors.ChatRoomNotFound);

            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active is null)
                return Result.Failure<PaginatedResult<RoomMessagesDto>>(ChatErrors.ActiveProfileRequired);

            var (clientProfileId, developerProfileId) = SplitActiveProfile(active.Value);
            var member = await _chatRoomMemberRepository.IsMember(clientProfileId, developerProfileId, ChatRoomId);
            if (member == null)
                return Result.Failure<PaginatedResult<RoomMessagesDto>>(ChatErrors.UserNotMember);

            var messages = _messageRepository.GetByRoomIdAsync(ChatRoomId);
            var result = messages.ToRoomMessageDto(clientProfileId, developerProfileId);
            var pagination = await PaginatedResult<RoomMessagesDto>.CreateAsync(
                result,
                pagedQuery.PageNumber,
                pagedQuery.PageSize);
            member.LastReadAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
            return Result.Success(pagination);
        }
    }
}
