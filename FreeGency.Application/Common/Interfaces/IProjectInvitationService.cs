using FreeGency.Application.Features.ProjectInvitations.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IProjectInvitationService
{
    Task<ApiResponse<ProjectInvitationDto>> CreateAsync(CreateProjectInvitationDto dto, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<ProjectInvitationDto>>> GetSentAsync(FilterProjectInvitationsDto filter, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<ProjectInvitationDto>>> GetReceivedAsync(FilterProjectInvitationsDto filter, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<ProjectInvitationDto>>> GetForTeamAsync(Guid teamId, FilterProjectInvitationsDto filter, CancellationToken ct = default);
    Task<ApiResponse<Guid>> AcceptAsync(Guid invitationId, CancellationToken ct = default);
    Task<ApiResponse> RejectAsync(Guid invitationId, CancellationToken ct = default);
    Task<ApiResponse> CancelAsync(Guid invitationId, CancellationToken ct = default);
}
