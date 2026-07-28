namespace FreeGency.Application.Features.Portfolio.DTOs;

public sealed class PortfolioCreatorDto
{
    public string Kind { get; init; } = "User"; // User | Team
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public string? Country { get; init; }
    public string? Headline { get; init; }
    public decimal AverageRating { get; init; }
    public int RatingCount { get; init; }
    public int MembersCount { get; init; }
    public IReadOnlyList<string> Specialties { get; init; } = [];
    public IReadOnlyList<string> Skills { get; init; } = [];
}

public sealed class OwnerReviewDto
{
    public Guid Id { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ReviewerUserId { get; init; }
    public string ReviewerName { get; init; } = string.Empty;
    public string? ReviewerAvatar { get; init; }
}
