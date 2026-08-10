using EntityFrameworkCore.EncryptColumn.Attribute;
using FreeGency.Domain.Abstractions;
using FreeGency.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FreeGency.Domain.Entities;

public class User : IdentityUser<Guid>, ISoftDeletableEntity
{
    public string FristName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public bool HasCompletedOnboarding { get; set; } = false;
    public profileMode? ActiveProfileMode { get; set; }
    public string? Country { get; set; }
    [EncryptColumn]
    public string? code { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    /// <summary>When set in the future, user cannot send chat/reviews until this UTC time.</summary>
    public DateTime? ModerationMutedUntil { get; set; }

    public virtual ClientProfile? ClientProfile { get; set; }
    public virtual ICollection<ModerationCase> ModerationCases { get; set; } = [];
    public virtual ICollection<UserModerationStrike> ModerationStrikes { get; set; } = [];
    public virtual DeveloperProfile? DeveloperProfile { get; set; }
    public virtual ICollection<Team> OwnedTeams { get; set; } = [];
    public virtual ICollection<Project> PostedProjects { get; set; } = [];
    public virtual ICollection<TeamMember> TeamMemberships { get; set; } = [];
    public virtual ICollection<ProjectProposal> ProjectProposals { get; set; } = [];
    public virtual ICollection<SavedProject> SavedProjects { get; set; } = [];
    public virtual ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = [];
    public virtual ICollection<SocialLink> SocialLinks { get; set; } = [];
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = [];
    public virtual ICollection<Review> ReviewsWritten { get; set; } = [];
    public virtual ICollection<Review> ReviewsReceived { get; set; } = [];
    public virtual ICollection<PortfolioProject> PortfolioProjects { get; set; } = [];
    public virtual ICollection<RecentlyViewedPortfolio> RecentlyViewedPortfolios { get; set; } = [];
    public virtual ICollection<PortfolioFeedback> PortfolioFeedbacks { get; set; } = [];
    public virtual ICollection<TeamFeedback> TeamFeedbacks { get; set; } = [];
    public virtual ICollection<DeveloperFeedback> DeveloperFeedbacks { get; set; } = [];
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<TeamPayoutSplit> TeamPayoutSplits { get; set; } = [];
    public virtual ICollection<TeamJob> CreatedTeamJobs { get; set; } = [];
    public virtual ICollection<ChatRoom> CreatedChatRooms { get; set; } = [];
    public virtual ICollection<ProjectFile> UploadedProjectFiles { get; set; } = [];
    public virtual ICollection<ProjectEvent> ProjectEvents { get; set; } = [];
    public virtual List<RefreshToken> refreshTokens { get; set; } = [];

}
