namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class UpdatePortfolioProjectRequestDto
    {
        public Guid Id { get; set; }
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public decimal? Budget { get; init; }

        public string? ProjectUrl { get; init; }

        public DateTime? CompletionDate { get; init; }

        public Guid? CategoryId { get; init; }

        public IEnumerable<Guid>? SkillIds { get; set; } = Enumerable.Empty<Guid>();

        public Visibility Visibility { get; init; }
    }
}
