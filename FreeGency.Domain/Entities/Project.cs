using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Entities;

public class Project : ISoftDeletableEntity
{
    public Guid Id { get; set; }
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
    public Guid? SpecialtyId { get; set; }
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

    public User Client { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Specialty? Specialty { get; set; }
    public Team? AssignedTeam { get; set; }
    public ICollection<ProjectSkill> ProjectSkills { get; set; } = [];
    public ICollection<ProjectProposal> ProjectProposals { get; set; } = [];
    public ICollection<SavedProject> SavedProjects { get; set; } = [];
    public ICollection<Milestone> Milestones { get; set; } = [];
    public ICollection<ProjectMember> ProjectMembers { get; set; } = [];
    public ICollection<ProjectFile> ProjectFiles { get; set; } = [];
    public ICollection<ProjectEvent> ProjectEvents { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ChatRoom> ChatRooms { get; set; } = [];
    public EscrowHold? EscrowHold { get; set; }
    public ICollection<TeamPayoutSplit> TeamPayoutSplits { get; set; } = [];
}
