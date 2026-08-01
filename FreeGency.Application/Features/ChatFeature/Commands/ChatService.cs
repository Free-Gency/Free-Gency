using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.ChatFeature.Mapping;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace FreeGency.Application.Features.ChatFeature.Commands
{
    public partial class ChatService(ICurrentUserService currentUserService,IUnitOfWork unitOfWork ,IStorageService storageService,IHttpContextAccessor httpContextAccessor) : IChatService
    {
        private readonly IProjectProposalRepository _projectProposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        private readonly IChatRoomRepository _chatRoomRepository = unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        private readonly ITeamMemberRepository _teamMemberRepository = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        private readonly IChatRoomMemberRepository _chatRoomMemberRepository = unitOfWork.Repository<IChatRoomMemberRepository, ChatRoomMember>();
        private readonly IMessageRepository _messageRepository = unitOfWork.Repository<IMessageRepository, Message>();

        public async Task<Result<RoomMessagesDto>> SendMessageAsync(Guid ChatRoomId, SendMessageRequest sendMessageRequest)
        {
            var roomIsExist = await _chatRoomRepository.GetByIdAsync(ChatRoomId);
            if (roomIsExist==null) return Result.Failure<RoomMessagesDto>(ChatErrors.ChatRoomNotFound);
            if (roomIsExist.Status == ChatRoomStatus.Archived) 
                 return Result.Failure<RoomMessagesDto>(ChatErrors.ChatRoomArchived);
            var userId = currentUserService.UserId;
            var member = await _chatRoomMemberRepository.IsMember(userId, ChatRoomId);
            if (member == null || !member.CanSend) return Result.Failure<RoomMessagesDto>(ChatErrors.CannotSendMessage);
            if (sendMessageRequest.File == null && string.IsNullOrWhiteSpace(sendMessageRequest.Text)) return Result.Failure<RoomMessagesDto>(ChatErrors.MessageCannotBeEmpty);
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
                SenderUserId = userId,
                Text = sendMessageRequest.Text,
                FileUrl = fileUrl,
                FileName = fileName,
                MessageType = sendMessageRequest.File != null
                                                ? MessageType.Attachment
                                                : MessageType.Text
            };

            await _messageRepository.AddAsync(message);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(new RoomMessagesDto
            {
                Id = message.Id,
                SenderId = userId,
                SenderName = $"{currentUserService.FirstName} {currentUserService.LastName}",
                MessageType = message.MessageType.ToString(),
                Text = message.Text,
                FileName = message.FileName,
                FileUrl = message.FileUrl,
                CreatedAt = message.CreatedAt,
                IsMine = true
            });
        }

        public async Task<Result<Guid>> StartDiscussionAsync(StartDiscussionRequestDto dto)
        {
            var userId = currentUserService.UserId;
            var proposal = await _projectProposalRepository.GetProposelById(dto.ProposalId);
            if (proposal == null) return Result.Failure<Guid>(ChatErrors.ProposalNotFound);
            if (proposal.Project.ClientId != userId) return Result.Failure<Guid>(ChatErrors.OnlyProjectClientCanStartDiscussion);
            if (proposal.Status != ProposalStatus.Pending)
                return Result.Failure<Guid>(ChatErrors.InvalidProposalStatus);
            var ChatRoomIsExist = await _chatRoomRepository.GetByProposalIdAsync(dto.ProposalId);
            if (ChatRoomIsExist != null) return Result.Failure<Guid>(ChatErrors.DiscussionAlreadyExists);
            var chatRoom = proposal.ToEntity(userId);
            await _chatRoomRepository.AddAsync(chatRoom);
            var members = new List<ChatRoomMember>
            {
                new()
                {
                    Id=Guid.NewGuid(),
                    ChatRoomId=chatRoom.Id,
                    UserId=userId,
                    RoleLabel="Client",
                    CanSend=true,
                    JoinedAt=DateTime.UtcNow
                }
            };
            if (proposal.TeamId != null)
            {
                var Leaders = await _teamMemberRepository.GetLeadersAsync(proposal.TeamId!.Value);
                if (Leaders.Count == 0)
                    return Result.Failure<Guid>(ChatErrors.TeamHasNoLeaders);
                AddLeaders(members, Leaders,proposal.UserId!.Value, chatRoom.Id);
            }
            else
            {
                members.Add(new()
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = chatRoom.Id,
                    UserId = proposal.UserId!.Value,
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
            return Result.Success(chatRoom.Id);
        }
        private void AddLeaders(List<ChatRoomMember> rm,IReadOnlyList<TeamMember> leaders,Guid proposalUserId, Guid roomId){
            foreach(var leader in leaders)
            {
                rm.Add( new ChatRoomMember
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = roomId,
                    RoleLabel = "Leader",
                    UserId = leader.UserId,
                    JoinedAt = DateTime.UtcNow,
                    CanSend = leader.UserId == proposalUserId
                }
                );
            }    
        }
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
