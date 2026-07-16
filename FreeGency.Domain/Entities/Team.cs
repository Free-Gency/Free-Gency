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
    public string TeamCode { get; set; } = string.Empty;
    public string? AboutUs { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;

    public User Owner { get; set; } = null!;
    public ICollection<TeamMember> TeamMembers { get; set; } = [];
    public ICollection<TeamCategory> TeamCategories { get; set; } = [];
    public ICollection<TeamSkill> TeamSkills { get; set; } = [];
    public ICollection<TeamJob> TeamJobs { get; set; } = [];
    public ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = [];
    public ICollection<SocialLink> SocialLinks { get; set; } = [];
    public ICollection<ProjectProposal> ProjectProposals { get; set; } = [];
    public ICollection<Project> AssignedProjects { get; set; } = [];
    public ICollection<TeamPayoutSplit> TeamPayoutSplits { get; set; } = [];
    public ICollection<ChatRoom> ChatRooms { get; set; } = [];
    public ICollection<PortfolioProject> PortfolioProjects { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
