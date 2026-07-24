namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class PortfolioProjectDetailsDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public decimal? Budget { get; init; }

        public string? ImageCover { get; init; }

        public string? ProjectUrl { get; init; }

        public DateTime? CompletionDate { get; init; }

        public Visibility Visibility { get; init; }

        public string? CategoryName { get; init; }

        public IEnumerable<PortfolioImageDto> Images { get; init; } = [];

        public IEnumerable<PortfolioSkillDto> Skills { get; init; } = [];
    }
}
