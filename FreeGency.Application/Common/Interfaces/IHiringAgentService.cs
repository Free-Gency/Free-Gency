using FreeGency.Application.Features.HiringAgent.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IHiringAgentService
{
    Task<ApiResponse<HiringAgentRunDto>> StartAsync(
        StartHiringAgentRunRequestDto request,
        CancellationToken ct = default);

    Task<ApiResponse<HiringAgentRunDto>> GetRunAsync(Guid runId, CancellationToken ct = default);

    Task<ApiResponse<HiringAgentRunDto>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);

    Task<ApiResponse<IReadOnlyList<HiringAgentRunDto>>> ListMineAsync(CancellationToken ct = default);

    Task<ApiResponse<HiringAgentReportDto>> GetReportAsync(Guid runId, CancellationToken ct = default);

    Task<ApiResponse> ConfirmHireAsync(Guid runId, Guid? candidateId = null, CancellationToken ct = default);

    Task<ApiResponse> DismissAsync(Guid runId, CancellationToken ct = default);

    Task<ApiResponse> CancelAsync(Guid runId, CancellationToken ct = default);

    /// <summary>
    /// Client finishes early: expire pending invites and rank whoever already accepted.
    /// </summary>
    Task<ApiResponse<HiringAgentRunDto>> CloseInvitesAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Hangfire entry: match + invite candidates.</summary>
    Task ProcessMatchAndInviteAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Hangfire: start/continue discussion agent for a candidate.</summary>
    Task ProcessDiscussionTurnAsync(Guid candidateId, CancellationToken ct = default);

    /// <summary>Hangfire: after invite accept — link candidate and kick off discussion.</summary>
    Task OnInvitationAcceptedAsync(Guid invitationId, Guid proposalId, Guid chatRoomId, CancellationToken ct = default);

    /// <summary>Hangfire: after invite reject.</summary>
    Task OnInvitationRejectedAsync(Guid invitationId, CancellationToken ct = default);

    /// <summary>Hangfire: evaluate deadlines / advance to ranking when ready.</summary>
    Task EvaluateRunProgressAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Hangfire: rank discussions and publish report.</summary>
    Task ProcessRankingAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Hangfire: freelancer sent a message — generate next agent reply if applicable.</summary>
    Task OnFreelancerMessageAsync(Guid chatRoomId, CancellationToken ct = default);

    /// <summary>
    /// Hangfire: after client hire approval, review a newly proposed plan and
    /// either request changes or accept (complete hire) automatically.
    /// </summary>
    Task ProcessPostHirePlanReviewAsync(Guid proposalId, Guid planVersionId, CancellationToken ct = default);

    Task<bool> HasActiveRunForProjectAsync(Guid projectId, CancellationToken ct = default);

    Task<int> GetActiveDiscussionLimitAsync(Guid projectId, CancellationToken ct = default);
}
