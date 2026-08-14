
namespace FreeGency.Domain.Entities;

public class Category : ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? ImageCover { get; set; }

    public virtual ICollection<CategorySpecialty> CategorySpecialties { get; set; } = [];
    public virtual ICollection<Project> Projects { get; set; } = [];
    public virtual ICollection<TeamCategory> TeamCategories { get; set; } = [];
    public virtual ICollection<UserInterest> UserInterests { get; set; } = [];
    public virtual ICollection<PortfolioProject> PortfolioProjects { get; set; } = [];
}
