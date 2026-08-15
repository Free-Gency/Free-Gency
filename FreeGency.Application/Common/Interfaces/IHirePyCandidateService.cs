namespace FreeGency.Application.Common.Interfaces
{
    public interface IHirePyCandidateService
    {
        Task ProcessRankingAndInvitationsAsync(Guid sessionId, CancellationToken ct = default);

        Task OnInvitationAcceptedAsync(Guid projectId, Guid proposalId, Guid freelancerUserId, CancellationToken ct = default);

        Task OnInvitationDeclinedAsync(Guid projectId, CancellationToken ct = default);
    }
}
