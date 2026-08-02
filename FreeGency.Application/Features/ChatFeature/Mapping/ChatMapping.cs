using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Domain.Enums;

namespace FreeGency.Application.Features.ChatFeature.Mapping
{
    public static class ChatMapping
    {
        public static ChatRoom ToEntity(this ProjectProposal projectProposal, Guid userId)
        {
            return new ChatRoom
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = userId,
                ProposalId = projectProposal.Id,
                TeamId = projectProposal.TeamId,
                Title = projectProposal.Project.Title,
                RoomType = RoomType.Proposal
            };
        }

        public static IQueryable<ChatRoomDto> ToChatRoomListDto(
            this IQueryable<ChatRoom> query,
            Guid? clientProfileId,
            Guid? developerProfileId)
        {
            return query
                .Select(r => new
                {
                    Room = r,

                    CurrentMember = r.ChatRoomMembers
                        .First(m =>
                            (clientProfileId != null && m.ClientProfileId == clientProfileId) ||
                            (developerProfileId != null && m.DeveloperProfileId == developerProfileId)),

                    LastMessage = r.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new
                        {
                            m.Text,
                            m.CreatedAt,
                            m.MessageType,
                            SenderName = m.SenderClientProfile != null
                                ? m.SenderClientProfile.User.FristName + " " + m.SenderClientProfile.User.LastName
                                : m.SenderDeveloperProfile != null
                                    ? m.SenderDeveloperProfile.User.FristName + " " + m.SenderDeveloperProfile.User.LastName
                                    : null,
                            m.SenderClientProfileId,
                            m.SenderDeveloperProfileId
                        })
                        .FirstOrDefault()
                })
                .Select(x => new ChatRoomDto
                {
                    Id = x.Room.Id,
                    Title = x.Room.Title!,
                    RoomType = x.Room.RoomType.ToString(),
                    Status = x.Room.Status.ToString(),

                    LastMessage = x.LastMessage != null
                        ? x.LastMessage.Text
                        : null,

                    LastMessageType = x.LastMessage != null
                        ? x.LastMessage.MessageType.ToString()
                        : null,

                    LastMessageSender = x.LastMessage != null
                        ? x.LastMessage.SenderName
                        : null,

                    LastMessageAt = x.LastMessage != null
                        ? x.LastMessage.CreatedAt
                        : null,

                    UnreadCount = x.Room.Messages.Count(m =>
                        !((clientProfileId != null && m.SenderClientProfileId == clientProfileId) ||
                          (developerProfileId != null && m.SenderDeveloperProfileId == developerProfileId)) &&
                        (x.CurrentMember.LastReadAt == null ||
                         m.CreatedAt > x.CurrentMember.LastReadAt)),
                    ArchivedAt=x.Room.ArchivedAt
                });
        }

        public static IQueryable<RoomMessagesDto> ToRoomMessageDto(
            this IQueryable<Message> messages,
            Guid? clientProfileId,
            Guid? developerProfileId)
        {
            return messages
                .OrderByDescending(x => x.CreatedAt)
                
                .OrderBy(x => x.CreatedAt)
                .Select(x => new RoomMessagesDto
                {
                Id = x.Id,
                SenderId = x.SenderClientProfileId ?? x.SenderDeveloperProfileId,
                SenderProfileType = x.SenderClientProfileId != null
                    ? nameof(profileMode.Client)
                    : x.SenderDeveloperProfileId != null
                        ? nameof(profileMode.Developer)
                        : null,
                SenderName = x.SenderClientProfile != null
                    ? x.SenderClientProfile.User.FristName + " " + x.SenderClientProfile.User.LastName
                    : x.SenderDeveloperProfile != null
                        ? x.SenderDeveloperProfile.User.FristName + " " + x.SenderDeveloperProfile.User.LastName
                        : null,
                Text = x.Text,
                FileName = x.FileName,
                FileUrl = x.FileUrl,
                CreatedAt = x.CreatedAt,
                MessageType = x.MessageType.ToString(),
                IsMine =
                    (clientProfileId != null && x.SenderClientProfileId == clientProfileId) ||
                    (developerProfileId != null && x.SenderDeveloperProfileId == developerProfileId)
            });
        }
    }
}
