
namespace FreeGency.Domain.Interfaces.Repositories;

public interface IProjectProposalRepository : IGenericRepository<ProjectProposal>
{
    Task<ProjectProposal?> GetProposelById(Guid Id);
    Task<IEnumerable<ProjectProposal>> GetByProjectIdAsync(Guid projectId,ProposalStatus? status = null,CancellationToken ct = default);
    Task<IEnumerable<ProjectProposal>> GetPendingByProjectIdAsync(Guid projectId,CancellationToken ct = default);
    Task<IEnumerable<ProjectProposal>> GetActiveDiscussionByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<IEnumerable<ProjectProposal>> GetCascadeRejectCandidatesAsync(Guid projectId, Guid exceptProposalId, CancellationToken ct = default);

    Task<IEnumerable<ProjectProposal>> GetByApplicantAsync(ApplicantType applicantType,Guid applicantId,ProposalStatus? status = null,CancellationToken ct = default);

    Task<bool> HasPendingOrActiveAsync(Guid projectId,ApplicantType applicantType,Guid applicantId,CancellationToken ct = default);

    Task AddWithAttachmentsAsync(ProjectProposal proposal,IEnumerable<ProposalAttachment> attachments,CancellationToken ct = default);

    Task UpdateStatusAsync(Guid proposalId,ProposalStatus status,CancellationToken ct = default);
    Task UpdateStatusAsync(Guid proposalId, ProposalStatus status, string? rejectReason, CancellationToken ct = default);

    Task AddAttachmentAsync(ProposalAttachment attachment,CancellationToken ct = default);

    Task DeleteAttachmentAsync(Guid attachmentId,CancellationToken ct = default);


    Task<int> CountSubmittedByUserSinceAsync(Guid userId, DateTime since, CancellationToken ct = default);

}
