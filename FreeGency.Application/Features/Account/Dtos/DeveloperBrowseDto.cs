namespace FreeGency.Application.Features.Account.Dtos;

public sealed class DeveloperBrowseDto
{
    public Guid UserId { get; init; }
    public Guid ProfileId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfileImage { get; init; }
    public string? Bio { get; init; }
    public string? Title { get; init; }
    public decimal AverageRating { get; init; }
    public int RatingCount { get; init; }
    public string Country { get; init; } = string.Empty;
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
}
