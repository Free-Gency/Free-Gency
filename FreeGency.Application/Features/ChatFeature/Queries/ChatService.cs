using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.ChatFeature.Mapping;
using FreeGency.Domain.Enums;

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

            if (filter.TeamId.HasValue)
            {
                var teamId = filter.TeamId.Value;
                var isMember = await _teamMemberRepository.IsMemberAsync(teamId, currentUserService.UserId);
                var team = await unitOfWork.Repository<ITeamRepository, Team>().GetByIdAsync(teamId);
                var isOwner = team != null && team.OwnerUserId == currentUserService.UserId;
                if (!isMember && !isOwner)
                    return Result.Failure<PaginatedResult<ChatRoomDto>>(ChatErrors.UserNotMember);

                query = query.Where(x => x.TeamId == teamId);
            }

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
                    .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt),
                filter.PageNumber,
                filter.PageSize
                );

            // Backfill ProjectId on the DTO only (Proposal rooms often keep ProjectId null
            // because IX_ChatRooms_ProjectId is unique — Project rooms claim that value later).
            var enriched = new List<ChatRoomDto>();
            foreach (var item in result.Items)
            {
                if (item.RoomType == nameof(RoomType.Proposal)
                    && (item.ProjectId is null || item.ProjectId == Guid.Empty)
                    && item.ProposalId is not null
                    && item.ProposalId != Guid.Empty)
                {
                    var proposal = await _projectProposalRepository.GetProposelById(item.ProposalId.Value);
                    if (proposal is not null)
                        item.ProjectId = proposal.ProjectId;
                }

                enriched.Add(item);
            }

            return Result.Success(
                PaginatedResult<ChatRoomDto>.FromList(
                    enriched,
                    result.PageNumber,
                    result.PageSize,
                    result.TotalCount));
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
            var members = await _chatRoomMemberRepository.GetRoomProfileIdsAsync(ChatRoomId);
            Guid? otherProfileId = null;
            if (members.Count() == 2)
            {
                foreach (var id in members)
                {
                    var profileId = member.ClientProfileId ?? member.DeveloperProfileId!.Value;
                    if (profileId != active.Value.ProfileId)
                    {
                        otherProfileId = profileId;
                        break;
                    }
                }
            }

            // Newest window first (page 1 = latest messages), then reverse for chronological UI.
            var newestFirst = messages
                .OrderByDescending(x => x.CreatedAt)
                .ToRoomMessageDto(clientProfileId, developerProfileId, otherProfileId);

            var pagination = await PaginatedResult<RoomMessagesDto>.CreateAsync(
                newestFirst,
                pagedQuery.PageNumber,
                pagedQuery.PageSize);

            var chronological = pagination.Items.Reverse().ToList();
            var result = PaginatedResult<RoomMessagesDto>.FromList(
                chronological,
                pagination.PageNumber,
                pagination.PageSize,
                pagination.TotalCount);

            member.LastReadAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
            return Result.Success(result);
        }
    }
}
