using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class ProjectProposal : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid ProjectId { get; set; }
    public ApplicantType ApplicantType { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? UserId { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public decimal ProposedBudget { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public DateTime AppliedAt { get; set; }
    public DateTime? ResponseAt { get; set; }

    public Project Project { get; set; } = null!;
    public Team? Team { get; set; }
    public User? User { get; set; }
    public ICollection<ProposalAttachment> ProposalAttachments { get; set; } = [];
    public ChatRoom? ChatRoom { get; set; }
}
