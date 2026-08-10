namespace FreeGency.AI.Suggestions;

public sealed class PortfolioSnippet
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<string> SkillNames { get; init; } = [];
}

public sealed class DeveloperSuggestionDocument
{
    public Guid UserId { get; init; }
    public string? Bio { get; init; }
    public decimal AverageRating { get; init; }
    public int RatingCount { get; init; }
    public IReadOnlyList<string> SkillNames { get; init; } = [];
    public IReadOnlyList<Guid> SkillIds { get; init; } = [];
    public IReadOnlyList<string> SpecialtyNames { get; init; } = [];
    public IReadOnlyList<Guid> SpecialtyIds { get; init; } = [];
    public IReadOnlyList<string> CategoryNames { get; init; } = [];
    public IReadOnlyList<Guid> CategoryIds { get; init; } = [];
    public IReadOnlyList<PortfolioSnippet> Portfolios { get; init; } = [];
}

public sealed class TeamSuggestionDocument
{
    public Guid TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AboutUs { get; init; }
    public decimal AverageRating { get; init; }
    public int RatingCount { get; init; }
    public bool HasOpenJobs { get; init; }
    public IReadOnlyList<string> SkillNames { get; init; } = [];
    public IReadOnlyList<Guid> SkillIds { get; init; } = [];
    public IReadOnlyList<string> SpecialtyNames { get; init; } = [];
    public IReadOnlyList<Guid> SpecialtyIds { get; init; } = [];
    public IReadOnlyList<string> CategoryNames { get; init; } = [];
    public IReadOnlyList<Guid> CategoryIds { get; init; } = [];
    public IReadOnlyList<PortfolioSnippet> Portfolios { get; init; } = [];
}

public sealed class TeamJobSuggestionDocument
{
    public Guid JobId { get; init; }
    public Guid TeamId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = "open";
    public string TeamName { get; init; } = string.Empty;
    public string? TeamAbout { get; init; }
    public decimal TeamAverageRating { get; init; }
    public int TeamRatingCount { get; init; }
    public bool TeamHasPublicPortfolio { get; init; }
    public IReadOnlyList<string> RequiredSkillNames { get; init; } = [];
    public IReadOnlyList<Guid> RequiredSkillIds { get; init; } = [];
    public IReadOnlyList<string> TeamSkillNames { get; init; } = [];
}

public sealed class ProjectSuggestionDocument
{
    public Guid ProjectId { get; init; }
    public Guid ClientId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = "open";
    public string? CategoryName { get; init; }
    public Guid CategoryId { get; init; }
    public decimal BudgetMin { get; init; }
    public decimal BudgetMax { get; init; }
    public string Currency { get; init; } = "USD";
    public IReadOnlyList<string> SkillNames { get; init; } = [];
    public IReadOnlyList<Guid> SkillIds { get; init; } = [];
    public IReadOnlyList<string> SpecialtyNames { get; init; } = [];
    public IReadOnlyList<Guid> SpecialtyIds { get; init; } = [];
}

public sealed class BuiltSuggestionDocument
{
    public required string Id { get; init; }
    public required string Collection { get; init; }
    public required string ContentType { get; init; }
    public required string Text { get; init; }
    public required Dictionary<string, string> Payload { get; init; }
}
