namespace FreeGency.Application.Features.Teams.Dtos;

public sealed class TeamReviewDto
{
    public Guid Id { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ReviewerUserId { get; init; }
    public string ReviewerName { get; init; } = string.Empty;
    public string? ReviewerAvatar { get; init; }
    public string? ModerationStatus { get; set; }
    public string? ModerationWarning { get; set; }
}

public sealed class CreateTeamFeedbackRequestDto
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
}
