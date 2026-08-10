namespace FreeGency.Application.Features.Account.Dtos;

public sealed class CreateDeveloperReviewRequestDto
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
}
