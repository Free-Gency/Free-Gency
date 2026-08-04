namespace FreeGency.Application.Features.Portfolio.DTOs;

public sealed class PortfolioRoadmapStepDto
{
    public Guid? Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsDone { get; init; }
}

public sealed class PortfolioMetricDto
{
    public Guid? Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}
