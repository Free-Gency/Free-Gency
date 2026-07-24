namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed record PortfolioImageDto(
        Guid Id,
        string ImageUrl,
        int SortOrder);
}
