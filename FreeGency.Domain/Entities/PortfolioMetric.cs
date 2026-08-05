namespace FreeGency.Domain.Entities;

public class PortfolioMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PortfolioProjectId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public virtual PortfolioProject PortfolioProject { get; set; } = null!;
}
