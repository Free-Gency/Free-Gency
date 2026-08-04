namespace FreeGency.Domain.Entities;

public class PortfolioRoadmapStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PortfolioProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsDone { get; set; }

    public virtual PortfolioProject PortfolioProject { get; set; } = null!;
}
