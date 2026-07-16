using FreeGency.Domain.Abstractions;

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

    public ICollection<Specialty> Specialties { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<TeamCategory> TeamCategories { get; set; } = [];
    public ICollection<UserInterest> UserInterests { get; set; } = [];
    public ICollection<PortfolioProject> PortfolioProjects { get; set; } = [];
}
