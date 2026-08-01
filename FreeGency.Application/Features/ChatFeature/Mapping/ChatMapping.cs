using FreeGency.Application.Features.ChatFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Mapping
{
    public static class ChatMapping
    {
        public static ChatRoom ToEntity(this ProjectProposal projectProposal,Guid UserId)
        {
            return new ChatRoom
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = UserId,
                ProposalId = projectProposal.Id,
                ProjectId = projectProposal.ProjectId,
                TeamId = projectProposal.TeamId,
                Title = projectProposal.Project.Title,
                RoomType = RoomType.Proposal,
                
            };
        }
        public static IQueryable<ChatRoomDto> ToChatRoomListDto(
    this IQueryable<ChatRoom> query,
    Guid currentUserId)
        {
            return query
                .Select(r => new
                {
                    Room = r,

                    CurrentMember = r.ChatRoomMembers
                        .First(m => m.UserId == currentUserId),

                    LastMessage = r.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new
                        {
                            m.Text,
                            m.CreatedAt,
                            m.MessageType,
                            SenderName = m.SenderUser != null
                                ? m.SenderUser.FristName + " " + m.SenderUser.LastName
                                : null,
                            m.SenderUserId
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
                        m.SenderUserId != currentUserId &&
                        (x.CurrentMember.LastReadAt == null ||
                         m.CreatedAt > x.CurrentMember.LastReadAt))
                });
        }
        public static IQueryable<RoomMessagesDto> ToRoomMessageDto(this IQueryable<Message> messages,Guid UserId)
        {
            return messages.Select(x => new RoomMessagesDto
            {
                Id = x.Id,
                SenderId = x.SenderUserId,
                SenderName = x.SenderUser != null ? x.SenderUser.FristName + " " + x.SenderUser.LastName : null,
                Text = x.Text,
                FileName=x.FileName,
                FileUrl = x.FileUrl,
                CreatedAt = x.CreatedAt,
                MessageType = x.MessageType.ToString(),
                IsMine = x.SenderUserId ==UserId
            });
        }
    }
}
