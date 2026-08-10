namespace FreeGency.Application.Features.Account.Dtos;

public class DeveloperAccountResponseDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public string? Bio { get; set; }

    public decimal AverageRating { get; set; }

    public int RatingCount { get; set; }

    public string Country { get; set; } = string.Empty;

    /// <summary>Primary specialty / role label for the portfolio header.</summary>
    public string? Title { get; set; }

    public bool IsAvailable { get; set; } = true;

    /// <summary>0–100 score derived from average rating when no dedicated metric exists.</summary>
    public int JobSuccessRate { get; set; }

    /// <summary>Completed portfolio / delivery count for the public profile.</summary>
    public int TotalJobs { get; set; }

    public List<ProfileInterestDto> Interests { get; set; } = [];
}
