namespace FreeGency.Application.Features.Portfolio.DTOs;

public sealed class PortfolioRoadmapStepDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsDone { get; set; }
}

public sealed class PortfolioMetricDto
{
    public Guid? Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
