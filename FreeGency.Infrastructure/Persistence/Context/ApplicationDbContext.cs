using EntityFrameworkCore.EncryptColumn.Extension;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using FreeGency.Domain.Entities;
using FreeGency.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IEncryptionProvider encryptionProvider)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<CategorySpecialty> CategorySpecialties => Set<CategorySpecialty>();
    public DbSet<SpecialtySkill> SpecialtySkills => Set<SpecialtySkill>();

    // Identity / profiles
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<DeveloperProfile> DeveloperProfiles => Set<DeveloperProfile>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    public DbSet<UserSpecialty> UserSpecialties => Set<UserSpecialty>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();

    // Teams
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamCategory> TeamCategories => Set<TeamCategory>();
    public DbSet<TeamSkill> TeamSkills => Set<TeamSkill>();
    public DbSet<TeamSpecialty> TeamSpecialties => Set<TeamSpecialty>();
    public DbSet<TeamJob> TeamJobs => Set<TeamJob>();
    public DbSet<TeamJobSkill> TeamJobSkills => Set<TeamJobSkill>();
    public DbSet<TeamJoinRequest> TeamJoinRequests => Set<TeamJoinRequest>();

    // Marketplace
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();
    public DbSet<ProjectSpecialty> ProjectSpecialties => Set<ProjectSpecialty>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectProposal> ProjectProposals => Set<ProjectProposal>();
    public DbSet<ProposalAttachment> ProposalAttachments => Set<ProposalAttachment>();
    public DbSet<ProjectFile> ProjectFiles => Set<ProjectFile>();
    public DbSet<ProjectEvent> ProjectEvents => Set<ProjectEvent>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<SavedProject> SavedProjects => Set<SavedProject>();
    public DbSet<Review> Reviews => Set<Review>();

    // Portfolio
    public DbSet<PortfolioProject> PortfolioProjects => Set<PortfolioProject>();
    public DbSet<PortfolioImage> PortfolioImages => Set<PortfolioImage>();
    public DbSet<PortfolioSkill> PortfolioSkills => Set<PortfolioSkill>();

    // Chat
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();
    public DbSet<Message> Messages => Set<Message>();

    // Finance
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<EscrowHold> EscrowHolds => Set<EscrowHold>();
    public DbSet<TeamPayoutSplit> TeamPayoutSplits => Set<TeamPayoutSplit>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseEncryption(encryptionProvider);
        modelBuilder.ApplyIdentitySchema();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyAuditableConfiguration();
        modelBuilder.ApplySoftDeleteConfiguration();
    }
}
