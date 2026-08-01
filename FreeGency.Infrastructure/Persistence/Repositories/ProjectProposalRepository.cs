

namespace FreeGency.Infrastructure.Persistence.Repositories;

public class ProjectProposalRepository
    : GenericRepository<ProjectProposal>, IProjectProposalRepository
{
    public ProjectProposalRepository(ApplicationDbContext context)
        : base(context)
    {
    }
    public async Task<IEnumerable<ProjectProposal>> GetByProjectIdAsync(Guid projectId,ProposalStatus? status = null,CancellationToken ct = default)
    {
        IQueryable<ProjectProposal> query = _dbSet.AsNoTracking().Where(p => p.ProjectId == projectId);
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        return await query.OrderByDescending(p => p.AppliedAt).ToListAsync(ct);
    }


    public async Task<IEnumerable<ProjectProposal>> GetPendingByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.ProjectId == projectId && p.Status == ProposalStatus.Pending)
            .OrderByDescending(p => p.AppliedAt)
            .ToListAsync(ct);
    }


    public async Task<IEnumerable<ProjectProposal>> GetByApplicantAsync(ApplicantType applicantType,Guid applicantId,ProposalStatus? status = null,CancellationToken ct = default)
    {
        IQueryable<ProjectProposal> query = _dbSet.AsNoTracking();
        query = applicantType switch
        {
            ApplicantType.User => query.Where(p => p.UserId == applicantId),
            ApplicantType.Team => query.Where(p => p.TeamId == applicantId),
            _ => throw new ArgumentOutOfRangeException(nameof(applicantType))
        };

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query.OrderByDescending(p => p.AppliedAt).ToListAsync(ct);
    }
    public async Task<bool> HasPendingOrActiveAsync(Guid projectId,ApplicantType applicantType,Guid applicantId,CancellationToken ct = default)
    {
        IQueryable<ProjectProposal> query = _dbSet.AsNoTracking().Where(p =>
            p.ProjectId == projectId &&
            (p.Status == ProposalStatus.Pending ||
             p.Status == ProposalStatus.Viewed ||
             p.Status == ProposalStatus.InDiscussion));

        query = applicantType switch
        {
            ApplicantType.User =>query.Where(p => p.UserId == applicantId),

            ApplicantType.Team =>query.Where(p => p.TeamId == applicantId),

            _ => throw new ArgumentOutOfRangeException(nameof(applicantType))
        };

        return await query.AnyAsync(ct);
    }

    public async Task AddWithAttachmentsAsync(ProjectProposal proposal,IEnumerable<ProposalAttachment> attachments,CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        if (proposal.Id == Guid.Empty)
            proposal.Id = Guid.NewGuid();
        proposal.AppliedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(proposal, ct);
        var files = attachments?.Distinct().ToList() ?? [];
        if (files.Count == 0)
            return;
        foreach (var file in files)
        {
            if (file.Id == Guid.Empty)
                file.Id = Guid.NewGuid();
            file.ProposalId = proposal.Id;
        }
        await _context.Set<ProposalAttachment>().AddRangeAsync(files, ct);
    }
    public async Task UpdateStatusAsync(Guid proposalId,ProposalStatus status,CancellationToken ct = default)
    {
        var proposal = await _dbSet.FirstOrDefaultAsync(p => p.Id == proposalId, ct);

        if (proposal is null)
            throw new KeyNotFoundException("Proposal not found.");
        proposal.Status = status;
        proposal.ResponseAt = DateTime.UtcNow;
       _dbSet.Update(proposal);
    }

    public async Task UpdateStatusAsync(
        Guid proposalId,
        ProposalStatus status,
        string? rejectReason,
        CancellationToken ct = default)
    {
        var proposal = await _dbSet.FirstOrDefaultAsync(p => p.Id == proposalId, ct);
        if (proposal is null)
            throw new KeyNotFoundException("Proposal not found.");
        proposal.Status = status;
        proposal.RejectReason = rejectReason;
        proposal.ResponseAt = DateTime.UtcNow;
        _dbSet.Update(proposal);
    }

    public async Task<IEnumerable<ProjectProposal>> GetActiveDiscussionByProjectIdAsync(
        Guid projectId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.ProjectId == projectId && p.Status == ProposalStatus.InDiscussion)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProjectProposal>> GetCascadeRejectCandidatesAsync(
        Guid projectId, Guid exceptProposalId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p =>
                p.ProjectId == projectId &&
                p.Id != exceptProposalId &&
                p.Status != ProposalStatus.Rejected &&
                p.Status != ProposalStatus.Withdrawn &&
                p.Status != ProposalStatus.Expired)
            .ToListAsync(ct);
    }

    public async Task AddAttachmentAsync(ProposalAttachment attachment,CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        if (attachment.Id == Guid.Empty)
            attachment.Id = Guid.NewGuid();
        if (!await ExistsAsync(attachment.ProposalId, ct))
            throw new KeyNotFoundException("Proposal not found.");
        await _context.Set<ProposalAttachment>().AddAsync(attachment, ct);
    }

    public async Task DeleteAttachmentAsync(Guid attachmentId,CancellationToken ct = default)
    {
        var attachment = await _context.Set<ProposalAttachment>().FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment is null)
            throw new KeyNotFoundException("Attachment not found.");
        _context.Set<ProposalAttachment>().Remove(attachment);
    }

    public async Task<ProjectProposal?> GetProposelById(Guid Id)
    {
        return await _dbSet.Include(x => x.Project).Where(x => x.Id == Id).FirstOrDefaultAsync();
    }
}