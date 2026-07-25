namespace FreeGency.Domain.Entities;

public class Project : ISoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsFixedPrice { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public int? EstimatedDurationDays { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public Guid? AssignedTeamId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual User Client { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual Team? AssignedTeam { get; set; }
    public virtual ICollection<ProjectSpecialty> ProjectSpecialties { get; set; } = [];
    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = [];
    public virtual ICollection<ProjectProposal> ProjectProposals { get; set; } = [];
    public virtual ICollection<SavedProject> SavedProjects { get; set; } = [];
    public virtual ICollection<Milestone> Milestones { get; set; } = [];
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = [];
    public virtual ICollection<ProjectFile> ProjectFiles { get; set; } = [];
    public virtual ICollection<ProjectEvent> ProjectEvents { get; set; } = [];
    public virtual ICollection<Review> Reviews { get; set; } = [];
    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = [];
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = [];
    public virtual EscrowHold? EscrowHold { get; set; }
    public virtual ICollection<TeamPayoutSplit> TeamPayoutSplits { get; set; } = [];
}
