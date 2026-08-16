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
            else
            {
                // Developers: team Proposal/Project chats live under Team → Messages.
                // Clients have no team inbox — include every room they belong to,
                // including discussions with applicant teams (TeamId set).
                if (active.Value.Mode == profileMode.Developer)
                {
                    query = query.Where(x =>
                        x.RoomType == RoomType.TeamMain
                        || x.RoomType == RoomType.TeamGroup
                        || x.TeamId == null);
                }
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

            // Page rooms first (cheap), then project last-message / unread only for that page.
            // Ordering the full DTO projection by LastMessageAt timed out under SQL Server.
            var totalCount = await query.CountAsync();
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

            var pageRoomIds = await query
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Id)
                .ToListAsync();

            var pageItems = pageRoomIds.Count == 0
                ? new List<ChatRoomDto>()
                : await _chatRoomRepository.Query()
                    .AsNoTracking()
                    .Where(r => pageRoomIds.Contains(r.Id))
                    .ToChatRoomListDto(clientProfileId, developerProfileId)
                    .ToListAsync();

            var byId = pageItems.ToDictionary(x => x.Id);
            var orderedItems = pageRoomIds
                .Where(id => byId.ContainsKey(id))
                .Select(id => byId[id])
                .ToList();

            var result = PaginatedResult<ChatRoomDto>.FromList(
                orderedItems,
                pageNumber,
                pageSize,
                totalCount);

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

                // Resolve 1:1 peer for online/offline presence.
                if (item.RoomType is nameof(RoomType.Proposal) or nameof(RoomType.Project))
                {
                    var members = await _chatRoomMemberRepository.GetRoomProfileIdsAsync(item.Id);
                    if (members.Count == 2)
                    {
                        foreach (var roomMember in members)
                        {
                            var profileId = roomMember.ClientProfileId ?? roomMember.DeveloperProfileId!.Value;
                            if (profileId != active.Value.ProfileId)
                            {
                                item.OtherProfileId = profileId;
                                break;
                            }
                        }
                    }
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
            if (members.Count == 2)
            {
                foreach (var roomMember in members)
                {
                    var profileId = roomMember.ClientProfileId ?? roomMember.DeveloperProfileId!.Value;
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
