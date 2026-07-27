namespace FreeGency.Application.Features.Portfolio.DTOs;

public sealed class CreatePortfolioFeedbackRequestDto
{
    public int Rating { get; init; }

    public string? Comment { get; init; }
}
