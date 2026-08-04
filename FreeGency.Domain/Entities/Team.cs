using FreeGency.Domain.Abstractions;

namespace FreeGency.Domain.Entities;

public class Team : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Cover { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string? AboutUs { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;

    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<TeamMember> TeamMembers { get; set; } = [];
    public virtual ICollection<TeamCategory> TeamCategories { get; set; } = [];
    public virtual ICollection<TeamSkill> TeamSkills { get; set; } = [];
    public virtual ICollection<TeamSpecialty> TeamSpecialties { get; set; } = [];
    public virtual ICollection<TeamJob> TeamJobs { get; set; } = [];
    public virtual ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = [];
    public virtual ICollection<SocialLink> SocialLinks { get; set; } = [];
    public virtual ICollection<ProjectProposal> ProjectProposals { get; set; } = [];
    public virtual ICollection<Project> AssignedProjects { get; set; } = [];
    public virtual ICollection<TeamPayoutSplit> TeamPayoutSplits { get; set; } = [];
    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = [];
    public virtual ICollection<PortfolioProject> PortfolioProjects { get; set; } = [];
    public virtual ICollection<Review> Reviews { get; set; } = [];
    public virtual Wallet? Wallet { get; set; }
}
