namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class UpdatePortfolioProjectRequestDto
    {
        public Guid Id { get; set; }
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public decimal? Budget { get; init; }

        public string? ProjectUrl { get; init; }

        public string? PrototypeUrl { get; init; }

        public DateTime? CompletionDate { get; init; }

        public Guid? CategoryId { get; init; }

        public IEnumerable<Guid>? SkillIds { get; set; } = Enumerable.Empty<Guid>();

        public Visibility Visibility { get; init; }

        public string? Challenge { get; init; }

        public string? Solution { get; init; }

        public string? DurationLabel { get; init; }

        public string? Industry { get; init; }

        public string? TeamLeads { get; init; }

        public string? TestimonialQuote { get; init; }

        public string? TestimonialAuthorName { get; init; }

        public string? TestimonialAuthorTitle { get; init; }

        public string? TestimonialAuthorAvatarUrl { get; init; }

        public List<PortfolioRoadmapStepDto>? RoadmapSteps { get; init; }

        public List<PortfolioMetricDto>? Metrics { get; init; }
    }
}
