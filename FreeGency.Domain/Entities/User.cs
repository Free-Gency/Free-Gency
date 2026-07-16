using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FreeGency.Domain.Entities;

public class User : IdentityUser<Guid>, ISoftDeletableEntity
{
    public string FristName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public profileMode? ActiveProfileMode { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ClientProfile? ClientProfile { get; set; }
    public DeveloperProfile? DeveloperProfile { get; set; }
    public ICollection<Team> OwnedTeams { get; set; } = [];
    public ICollection<Project> PostedProjects { get; set; } = [];
    public ICollection<TeamMember> TeamMemberships { get; set; } = [];
    public ICollection<ProjectProposal> ProjectProposals { get; set; } = [];
    public ICollection<UserSkill> UserSkills { get; set; } = [];
    public ICollection<UserInterest> UserInterests { get; set; } = [];
    public ICollection<SavedProject> SavedProjects { get; set; } = [];
    public ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = [];
    public ICollection<SocialLink> SocialLinks { get; set; } = [];
    public ICollection<ProjectMember> ProjectMembers { get; set; } = [];
    public ICollection<ChatRoomMember> ChatRoomMembers { get; set; } = [];
    public ICollection<Message> SentMessages { get; set; } = [];
    public ICollection<Review> ReviewsWritten { get; set; } = [];
    public ICollection<Review> ReviewsReceived { get; set; } = [];
    public ICollection<PortfolioProject> PortfolioProjects { get; set; } = [];
    public ICollection<TeamPayoutSplit> TeamPayoutSplits { get; set; } = [];
    public ICollection<TeamJob> CreatedTeamJobs { get; set; } = [];
    public ICollection<ChatRoom> CreatedChatRooms { get; set; } = [];
    public ICollection<ProjectFile> UploadedProjectFiles { get; set; } = [];
    public ICollection<ProjectEvent> ProjectEvents { get; set; } = [];
}
