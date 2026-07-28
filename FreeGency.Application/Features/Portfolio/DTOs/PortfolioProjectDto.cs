namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed record PortfolioProjectDto(
        Guid Id,
        string Title,
        string Description,
        decimal? Budget,
        string? ImageCover,
        string? ProjectUrl,
        DateTime? CompletionDate,
        Visibility Visibility,
        string? CategoryName,
        string? OwnerName);
}
