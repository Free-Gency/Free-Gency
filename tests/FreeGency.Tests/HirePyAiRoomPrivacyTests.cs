using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.ChatFeature.Commands;
using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FreeGency.Tests;

/// <summary>
/// The HirePy AI discussion room is shared only by the freelancer and the AI bot,
/// so the existing chat authorization (membership checks) must already keep the
/// client out. These tests exercise the real ChatService against that room.
/// </summary>
public class HirePyAiRoomPrivacyTests
{
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _clientProfileId = Guid.NewGuid();

    private ChatService BuildChatService(Mock<IChatRoomRepository> chatRoomRepo, Mock<IChatRoomMemberRepository> memberRepo)
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetActiveProfileAsync(_clientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_clientProfileId, profileMode.Client));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IChatRoomRepository, ChatRoom>()).Returns(chatRoomRepo.Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.Repository<IChatRoomMemberRepository, ChatRoomMember>()).Returns(memberRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectProposalRepository, ProjectProposal>()).Returns(new Mock<IProjectProposalRepository>().Object);
        unitOfWork.Setup(u => u.Repository<ITeamMemberRepository, TeamMember>()).Returns(new Mock<ITeamMemberRepository>().Object);
        unitOfWork.Setup(u => u.Repository<IMessageRepository, Message>()).Returns(new Mock<IMessageRepository>().Object);
        unitOfWork.Setup(u => u.Repository<INotificationRepository, Notification>()).Returns(new Mock<INotificationRepository>().Object);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(_clientUserId);

        return new ChatService(
            currentUser.Object,
            unitOfWork.Object,
            null!,
            new Mock<IHubContext<ChatHub>>().Object,
            new Mock<INotificationService>().Object,
            new Mock<IContentModerationService>().Object);
    }

    [Fact]
    public async Task ClientCannotReadAiRoom_WhenNotAMember_ReturnsUserNotMember()
    {
        var chatRoomRepo = new Mock<IChatRoomRepository>();
        chatRoomRepo.Setup(r => r.RoomIsExist(_roomId)).ReturnsAsync(true);

        var memberRepo = new Mock<IChatRoomMemberRepository>();
        memberRepo.Setup(r => r.IsMember(_clientProfileId, null, _roomId)).ReturnsAsync((ChatRoomMember?)null);

        var service = BuildChatService(chatRoomRepo, memberRepo);

        var result = await service.GetMessageChatRoom(_roomId, new RoomMessageFilter());

        Assert.False(result.IsSuccess);
        Assert.Equal(ChatErrors.UserNotMember, result.error);
    }

    [Fact]
    public async Task ClientCannotSendInAiRoom_WhenNotAMember_ReturnsCannotSendMessage()
    {
        var chatRoomRepo = new Mock<IChatRoomRepository>();
        chatRoomRepo.Setup(r => r.GetByIdAsync(_roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatRoom { Id = _roomId, Status = ChatRoomStatus.Active });

        var memberRepo = new Mock<IChatRoomMemberRepository>();
        memberRepo.Setup(r => r.IsMember(_clientProfileId, null, _roomId)).ReturnsAsync((ChatRoomMember?)null);

        var service = BuildChatService(chatRoomRepo, memberRepo);

        var result = await service.SendMessageAsync(_roomId, new SendMessageRequest { Text = "Show me this chat" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ChatErrors.CannotSendMessage, result.error);
    }

    [Fact]
    public async Task FreelancerIsAMember_CanActInAiRoom()
    {
        var freelancerUserId = Guid.NewGuid();
        var freelancerProfileId = Guid.NewGuid();

        var memberRepo = new Mock<IChatRoomMemberRepository>();
        memberRepo.Setup(r => r.IsMember(null, freelancerProfileId, _roomId))
            .ReturnsAsync(new ChatRoomMember
            {
                Id = Guid.NewGuid(),
                ChatRoomId = _roomId,
                DeveloperProfileId = freelancerProfileId,
                CanSend = true
            });

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetActiveProfileAsync(freelancerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((freelancerProfileId, profileMode.Developer));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IChatRoomRepository, ChatRoom>()).Returns(new Mock<IChatRoomRepository>().Object);
        unitOfWork.Setup(u => u.Repository<IUserRepository, User>()).Returns(userRepo.Object);
        unitOfWork.Setup(u => u.Repository<IChatRoomMemberRepository, ChatRoomMember>()).Returns(memberRepo.Object);
        unitOfWork.Setup(u => u.Repository<IProjectProposalRepository, ProjectProposal>()).Returns(new Mock<IProjectProposalRepository>().Object);
        unitOfWork.Setup(u => u.Repository<ITeamMemberRepository, TeamMember>()).Returns(new Mock<ITeamMemberRepository>().Object);
        unitOfWork.Setup(u => u.Repository<IMessageRepository, Message>()).Returns(new Mock<IMessageRepository>().Object);
        unitOfWork.Setup(u => u.Repository<INotificationRepository, Notification>()).Returns(new Mock<INotificationRepository>().Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(freelancerUserId);

        var service = new ChatService(
            currentUser.Object,
            unitOfWork.Object,
            null!,
            new Mock<IHubContext<ChatHub>>().Object,
            new Mock<INotificationService>().Object,
            new Mock<IContentModerationService>().Object);

        var result = await service.MarkAsRead(_roomId);

        Assert.True(result.IsSuccess);
        memberRepo.Verify(r => r.IsMember(null, freelancerProfileId, _roomId), Times.Once);
    }
}
