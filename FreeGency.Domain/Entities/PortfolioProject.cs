namespace FreeGency.Domain.Entities;

public class PortfolioProject : ISoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public owner OwnerType { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public string? ImageCover { get; set; }
    public string? ProjectUrl { get; set; }
    public string? PrototypeUrl { get; set; }
    public DateTime? CompletionDate { get; set; }
    public Guid? CategoryId { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;

    public string? Challenge { get; set; }
    public string? Solution { get; set; }
    public string? DurationLabel { get; set; }
    public string? Industry { get; set; }
    public string? TeamLeads { get; set; }
    public string? TestimonialQuote { get; set; }
    public string? TestimonialAuthorName { get; set; }
    public string? TestimonialAuthorTitle { get; set; }
    public string? TestimonialAuthorAvatarUrl { get; set; }

    public virtual User? OwnerUser { get; set; }
    public virtual Team? OwnerTeam { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ICollection<PortfolioImage> PortfolioImages { get; set; } = [];
    public virtual ICollection<PortfolioSkill> PortfolioSkills { get; set; } = [];
    public virtual ICollection<PortfolioRoadmapStep> RoadmapSteps { get; set; } = [];
    public virtual ICollection<PortfolioMetric> Metrics { get; set; } = [];
    public virtual ICollection<RecentlyViewedPortfolio> RecentlyViewedByUsers { get; set; } = [];
    public virtual ICollection<PortfolioFeedback> Feedbacks { get; set; } = [];
}
