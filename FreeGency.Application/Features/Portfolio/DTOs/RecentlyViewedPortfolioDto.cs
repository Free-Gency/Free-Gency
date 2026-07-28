namespace FreeGency.Application.Features.Portfolio.DTOs;

public sealed record RecentlyViewedPortfolioDto(
    Guid Id,
    string Title,
    string? CategoryName,
    string? OwnerName,
    string? ImageCover,
    DateTime ViewedAt);
