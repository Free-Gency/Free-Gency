using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.ProjectInvitations;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Interfaces;
using Hangfire;
using Moq;

namespace FreeGency.Tests;

public class ProjectInvitationServiceTests
{
    [Fact]
    public async Task ExpireOverdueInvitations_MarksOnlyOverduePendingInvitationsAsExpired()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Status = ProjectInvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Status = ProjectInvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Status = ProjectInvitationStatus.Accepted,
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            },
        };

        var invitationRepo = new Mock<IProjectInvitationRepository>();
        invitationRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ProjectInvitation>(invitations));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IProjectInvitationRepository, ProjectInvitation>()).Returns(invitationRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new ProjectInvitationService(
            new Mock<ICurrentUserService>().Object,
            unitOfWork.Object,
            new Mock<INotificationService>().Object,
            new Mock<IBackgroundJobClient>().Object);

        var result = await service.ExpireOverdueInvitationsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data);
        Assert.Equal(ProjectInvitationStatus.Expired, invitations[0].Status);
        Assert.Equal(ProjectInvitationStatus.Pending, invitations[1].Status);
        Assert.Equal(ProjectInvitationStatus.Accepted, invitations[2].Status);
    }

    [Fact]
    public async Task ExpireOverdueInvitations_WithNoOverdue_ReturnsZero()
    {
        var invitations = new List<ProjectInvitation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Status = ProjectInvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            },
        };

        var invitationRepo = new Mock<IProjectInvitationRepository>();
        invitationRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ProjectInvitation>(invitations));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<IProjectInvitationRepository, ProjectInvitation>()).Returns(invitationRepo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new ProjectInvitationService(
            new Mock<ICurrentUserService>().Object,
            unitOfWork.Object,
            new Mock<INotificationService>().Object,
            new Mock<IBackgroundJobClient>().Object);

        var result = await service.ExpireOverdueInvitationsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data);
        Assert.Equal(ProjectInvitationStatus.Pending, invitations[0].Status);
    }
}
