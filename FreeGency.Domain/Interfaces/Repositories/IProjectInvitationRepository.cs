using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories;

public interface IProjectInvitationRepository : IGenericRepository<ProjectInvitation>
{
    Task<ProjectInvitation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<bool> HasPendingAsync(
        Guid projectId,
        ApplicantType inviteeType,
        Guid inviteeId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ProjectInvitation>> GetSentByClientAsync(
        Guid clientUserId,
        ProjectInvitationStatus? status,
        CancellationToken ct = default);

    Task<IReadOnlyList<ProjectInvitation>> GetReceivedForDeveloperAsync(
        Guid developerUserId,
        IReadOnlyList<Guid> leaderTeamIds,
        ProjectInvitationStatus? status,
        CancellationToken ct = default);

    Task<IReadOnlyList<ProjectInvitation>> GetForTeamAsync(
        Guid teamId,
        ProjectInvitationStatus? status,
        CancellationToken ct = default);

    Task<IReadOnlyList<ProjectInvitation>> GetPendingByProjectIdAsync(
        Guid projectId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ProjectInvitation>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken ct = default);
}
