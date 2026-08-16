namespace FreeGency.Domain.Interfaces.Repositories;

public interface IHiringAgentRunRepository : IGenericRepository<HiringAgentRun>
{
    Task<HiringAgentRun?> GetByIdWithCandidatesAsync(Guid id, CancellationToken ct = default);

    Task<HiringAgentRun?> GetActiveByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    Task<bool> HasActiveRunForProjectAsync(Guid projectId, CancellationToken ct = default);

    Task<IReadOnlyList<HiringAgentRun>> GetByClientAsync(Guid clientUserId, CancellationToken ct = default);

    Task<HiringAgentRun?> GetByInvitationIdAsync(Guid invitationId, CancellationToken ct = default);

    Task<HiringAgentCandidate?> GetCandidateByChatRoomIdAsync(Guid chatRoomId, CancellationToken ct = default);

    Task<HiringAgentRun?> GetByCandidateIdWithDetailsAsync(Guid candidateId, CancellationToken ct = default);

    /// <summary>
    /// ReportReady run where the client already approved hire for this proposal
    /// (agent may auto-review revised plans).
    /// </summary>
    Task<HiringAgentRun?> GetClientApprovedReportReadyByProposalIdAsync(
        Guid proposalId,
        CancellationToken ct = default);

    Task PersistMatchInviteResultsAsync(
        Guid runId,
        IReadOnlyList<HiringAgentCandidate> candidates,
        CancellationToken ct = default);
}
