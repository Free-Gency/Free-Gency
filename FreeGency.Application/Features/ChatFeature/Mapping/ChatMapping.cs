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
                // Null on purpose — unique ProjectId index is reserved for Project rooms after hire.
                ProjectId = null,
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

                    ClientName =
                        r.Project != null
                            ? (r.Project.Client.FristName + " " + r.Project.Client.LastName).Trim()
                            : r.Proposal != null && r.Proposal.Project != null
                                ? (r.Proposal.Project.Client.FristName + " " + r.Proposal.Project.Client.LastName).Trim()
                                : null,

                    TeamName = r.Team != null ? r.Team.Name : null,
                    TeamLogo = r.Team != null ? r.Team.Logo : null,

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
                    ClientName = string.IsNullOrWhiteSpace(x.ClientName) ? null : x.ClientName,
                    TeamId = x.Room.TeamId,
                    TeamName = x.TeamName,
                    TeamLogo = x.TeamLogo,
                    Logo = x.Room.Logo,
                    // Prefer Proposal.ProjectId for negotiation rooms (legacy rows often left ProjectId null).
                    ProjectId = x.Room.Proposal != null
                        ? x.Room.Proposal.ProjectId
                        : x.Room.ProjectId,
                    ProposalId = x.Room.ProposalId ?? (x.Room.Proposal != null ? x.Room.Proposal.Id : null),
                    RoomType = x.Room.RoomType.ToString(),
                    Status = x.Room.Status.ToString(),
                    CreatedAt = x.Room.CreatedAt,
                    CanSend = x.CurrentMember.CanSend && x.Room.Status != ChatRoomStatus.Archived,
                    RoleLabel = x.CurrentMember.RoleLabel,

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
                    ArchivedAt = x.Room.ArchivedAt
                });
        }

        public static IQueryable<RoomMessagesDto> ToRoomMessageDto(
            this IQueryable<Message> messages,
            Guid? clientProfileId,
            Guid? developerProfileId,
            Guid? otherProfileId)
        {
            return messages
                .Select(x => new RoomMessagesDto
                {
                    Id = x.Id,
                    ChatRoomId = x.ChatRoomId,
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
                    Text =
                        x.ModerationStatus == ModerationStatus.Visible
                            ? x.Text
                            : ((clientProfileId != null && x.SenderClientProfileId == clientProfileId) ||
                               (developerProfileId != null && x.SenderDeveloperProfileId == developerProfileId))
                                ? x.Text
                                : (x.ModeratedText ?? "Message removed by FreeGency for a policy violation."),
                    FileName = x.ModerationStatus == ModerationStatus.Hidden
                        && !((clientProfileId != null && x.SenderClientProfileId == clientProfileId) ||
                             (developerProfileId != null && x.SenderDeveloperProfileId == developerProfileId))
                            ? null
                            : x.FileName,
                    FileUrl = x.ModerationStatus == ModerationStatus.Hidden
                        && !((clientProfileId != null && x.SenderClientProfileId == clientProfileId) ||
                             (developerProfileId != null && x.SenderDeveloperProfileId == developerProfileId))
                            ? null
                            : x.FileUrl,
                    PlanVersionId = x.PlanVersionId,
                    MilestoneId = x.MilestoneId,
                    CreatedAt = x.CreatedAt,
                    MessageType = x.MessageType.ToString(),
                    IsMine =
                        (clientProfileId != null && x.SenderClientProfileId == clientProfileId) ||
                        (developerProfileId != null && x.SenderDeveloperProfileId == developerProfileId),
                    OtherProfileId = otherProfileId,
                    ModerationStatus = x.ModerationStatus.ToString(),
                    ModerationWarning =
                        ((clientProfileId != null && x.SenderClientProfileId == clientProfileId) ||
                         (developerProfileId != null && x.SenderDeveloperProfileId == developerProfileId))
                            ? x.ModerationNote
                            : null
                });
        }
    }
}
