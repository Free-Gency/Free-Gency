namespace FreeGency.Application.Features.Account.Dtos;

public sealed class DeveloperReviewDto
{
    public Guid Id { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ReviewerUserId { get; init; }
    public string ReviewerName { get; init; } = string.Empty;
    public string? ReviewerTitle { get; init; }
    public string? ReviewerAvatar { get; init; }
    public string? ModerationStatus { get; set; }
    public string? ModerationWarning { get; set; }
}
