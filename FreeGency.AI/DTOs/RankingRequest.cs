namespace FreeGency.AI.DTOs;

public sealed class RankingRequest
{
    public required string ProjectDescription { get; init; }
    public required IReadOnlyList<RankingCandidate> Candidates { get; init; }
    public IReadOnlyList<string>? RequiredSkills { get; init; }
    public string? Budget { get; init; }
    public string? Timeline { get; init; }
    public int TopK { get; init; } = 5;
}

public sealed class RankingCandidate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? ProfileSummary { get; init; }
    public IReadOnlyList<string>? Skills { get; init; }
    public decimal? HourlyRate { get; init; }
    public double? Rating { get; init; }
    public int? CompletedProjects { get; init; }
}
