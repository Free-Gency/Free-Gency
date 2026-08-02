using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.ChatFeature.Mapping;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using Microsoft.AspNetCore.SignalR;

namespace FreeGency.Application.Features.ChatFeature.Commands
{
    public partial class ChatService(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IStorageService storageService
        , IHubContext<ChatHub> hub
        ) : IChatService
    {
        private readonly IProjectProposalRepository _projectProposalRepository =
            unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        private readonly IChatRoomRepository _chatRoomRepository =
            unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        private readonly ITeamMemberRepository _teamMemberRepository =
            unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        private readonly IChatRoomMemberRepository _chatRoomMemberRepository =
            unitOfWork.Repository<IChatRoomMemberRepository, ChatRoomMember>();
        private readonly IMessageRepository _messageRepository =
            unitOfWork.Repository<IMessageRepository, Message>();
        private readonly IUserRepository _userRepository =
            unitOfWork.Repository<IUserRepository, User>();

        public async Task<Result<RoomMessagesDto>> SendMessageAsync(Guid ChatRoomId, SendMessageRequest sendMessageRequest)
        {
            var roomIsExist = await _chatRoomRepository.GetByIdAsync(ChatRoomId);
            if (roomIsExist == null) return Result.Failure<RoomMessagesDto>(ChatErrors.ChatRoomNotFound);
            if (roomIsExist.Status == ChatRoomStatus.Archived)
                return Result.Failure<RoomMessagesDto>(ChatErrors.ChatRoomArchived);

            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active is null) return Result.Failure<RoomMessagesDto>(ChatErrors.ActiveProfileRequired);

            var (clientProfileId, developerProfileId) = SplitActiveProfile(active.Value);
            var member = await _chatRoomMemberRepository.IsMember(clientProfileId, developerProfileId, ChatRoomId);
            if (member == null || !member.CanSend) return Result.Failure<RoomMessagesDto>(ChatErrors.CannotSendMessage);
            if (sendMessageRequest.File == null && string.IsNullOrWhiteSpace(sendMessageRequest.Text))
                return Result.Failure<RoomMessagesDto>(ChatErrors.MessageCannotBeEmpty);

            string? fileUrl = null;
            string? fileName = null;

            if (sendMessageRequest.File is not null)
            {
                (fileUrl, fileName) = await SaveFile(sendMessageRequest.File);

                if (fileUrl is null)
                    return Result.Failure<RoomMessagesDto>(FileErrors.UploadFailed);
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = ChatRoomId,
                SenderClientProfileId = clientProfileId,
                SenderDeveloperProfileId = developerProfileId,
                Text = sendMessageRequest.Text,
                FileUrl = fileUrl,
                FileName = fileName,
                MessageType = sendMessageRequest.File != null
                    ? MessageType.Attachment
                    : MessageType.Text
            };

            await _messageRepository.AddAsync(message);
            member.LastReadAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
            var roomMembers = await _chatRoomMemberRepository.GetRoomProfileIdsAsync(ChatRoomId);
            var dto = new RoomMessagesDto
            {
                Id = message.Id,
                SenderId = active.Value.ProfileId,
                SenderProfileType = active.Value.Mode.ToString(),
                SenderName = $"{currentUserService.FirstName} {currentUserService.LastName}",
                MessageType = message.MessageType.ToString(),
                Text = message.Text,
                FileName = message.FileName,
                FileUrl = message.FileUrl,
                CreatedAt = message.CreatedAt,
                IsMine = true
            };
            foreach (var profileId in roomMembers)
            {
                await hub.Clients
                    .Group($"profile-{profileId}")
                    .SendAsync("ReceiveMessage", dto);
            }
            var roomUpdated = new RoomUpdatedDto
            {
                RoomId = ChatRoomId,
                LastMessage = message.Text ?? message.FileName,
                LastMessageType = message.MessageType.ToString(),
                LastMessageAt = message.CreatedAt,
                LastMessageSender = $"{currentUserService.FirstName} {currentUserService.LastName}",
                SenderId= active.Value.ProfileId
            };
            foreach (var profileId in roomMembers)
            {
                await hub.Clients
                    .Group($"profile-{profileId}")
                    .SendAsync("RoomUpdated", roomUpdated);
            }
            return Result.Success(dto);
        }

        public async Task<Result<Guid>> StartDiscussionAsync(StartDiscussionRequestDto dto)
        {
            var userId = currentUserService.UserId;
            var proposal = await _projectProposalRepository.GetProposelById(dto.ProposalId);
            if (proposal == null) return Result.Failure<Guid>(ChatErrors.ProposalNotFound);
            if (proposal.Project.ClientId != userId)
                return Result.Failure<Guid>(ChatErrors.OnlyProjectClientCanStartDiscussion);
            if (proposal.Status != ProposalStatus.Pending)
                return Result.Failure<Guid>(ChatErrors.InvalidProposalStatus);

            var chatRoomIsExist = await _chatRoomRepository.GetByProposalIdAsync(dto.ProposalId);
            if (chatRoomIsExist != null) return Result.Failure<Guid>(ChatErrors.DiscussionAlreadyExists);

            var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(userId);
            if (clientProfileId is null) return Result.Failure<Guid>(ChatErrors.ProfileNotFound);

            var chatRoom = proposal.ToEntity(userId);
            await _chatRoomRepository.AddAsync(chatRoom);

            var members = new List<ChatRoomMember>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = chatRoom.Id,
                    ClientProfileId = clientProfileId,
                    RoleLabel = "Client",
                    CanSend = true,
                    JoinedAt = DateTime.UtcNow
                }
            };

            if (proposal.TeamId != null)
            {
                var leaders = await _teamMemberRepository.GetLeadersAsync(proposal.TeamId!.Value);
                if (leaders.Count == 0)
                    return Result.Failure<Guid>(ChatErrors.TeamHasNoLeaders);

                var leadersResult = await AddLeadersAsync(members, leaders, proposal.UserId!.Value, chatRoom.Id);
                if (leadersResult.isFailure)
                    return Result.Failure<Guid>(leadersResult.error);
            }
            else
            {
                var freelancerProfileId =
                    await _userRepository.GetDeveloperProfileIdByUserIdAsync(proposal.UserId!.Value);
                if (freelancerProfileId is null)
                    return Result.Failure<Guid>(ChatErrors.ProfileNotFound);

                members.Add(new()
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = chatRoom.Id,
                    DeveloperProfileId = freelancerProfileId,
                    RoleLabel = "Freelancer",
                    CanSend = true,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _chatRoomMemberRepository.AddRangeAsync(members);
            var message = new Message
            {
                Id = Guid.NewGuid(),
                MessageType = MessageType.System,
                ChatRoomId = chatRoom.Id,
                Text = "Discussion started. You can now negotiate the milestone plan."
            };
            await _messageRepository.AddAsync(message);
            await unitOfWork.SaveChangesAsync();
            var roomDto = new ChatRoomDto
            {
                Id = chatRoom.Id,
                RoomType = chatRoom.RoomType.ToString(),
                Status = chatRoom.Status.ToString(),
                Title = chatRoom.Title,
                LastMessage = message.Text,
                LastMessageType = message.MessageType.ToString(),
                LastMessageAt = message.CreatedAt,
                LastMessageSender = "System",
                UnreadCount = 0,
                ArchivedAt = chatRoom.ArchivedAt
            };
            foreach (var member in members)
            {
                var profileId = member.ClientProfileId ?? member.DeveloperProfileId;

                await hub.Clients
                    .Group($"profile-{profileId}")
                    .SendAsync("DiscussionStarted", roomDto);
            }
            return Result.Success(chatRoom.Id);
        }
        public async Task<Result> MarkAsRead(Guid roomId)
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active is null)
                return Result.Failure(ChatErrors.ActiveProfileRequired);

            var (clientProfileId, developerProfileId) = SplitActiveProfile(active.Value);

            var member = await _chatRoomMemberRepository.IsMember(
                clientProfileId,
                developerProfileId,
                roomId);

            if (member == null)
                return Result.Failure(ChatErrors.UserNotMember);

            member.LastReadAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        private async Task<Result> AddLeadersAsync(
            List<ChatRoomMember> rm,
            IReadOnlyList<TeamMember> leaders,
            Guid proposalUserId,
            Guid roomId)
        {
            foreach (var leader in leaders)
            {
                var developerProfileId =
                    await _userRepository.GetDeveloperProfileIdByUserIdAsync(leader.UserId);
                if (developerProfileId is null)
                    return Result.Failure(ChatErrors.ProfileNotFound);

                rm.Add(new ChatRoomMember
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = roomId,
                    RoleLabel = "Leader",
                    DeveloperProfileId = developerProfileId,
                    JoinedAt = DateTime.UtcNow,
                    CanSend = leader.UserId == proposalUserId
                });
            }

            return Result.Success();
        }

        private static (Guid? ClientProfileId, Guid? DeveloperProfileId) SplitActiveProfile(
            (Guid ProfileId, profileMode Mode) active)
            => active.Mode == profileMode.Client
                ? (active.ProfileId, null)
                : (null, active.ProfileId);

        private async Task<(string? FileUrl, string? FileName)> SaveFile(IFormFile file)
        {
            try
            {
                var uploaded = await storageService.UploadAsync(
                    file,
                    StorageFolders.ChatFiles);

                return (uploaded.Url, uploaded.FileName);
            }
            catch
            {
                return (null, null);
            }
        }

       
    }
}
