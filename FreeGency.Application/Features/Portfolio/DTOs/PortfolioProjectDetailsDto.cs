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

        public string? PrototypeUrl { get; init; }

        public DateTime? CompletionDate { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public Visibility Visibility { get; init; }

        public string? CategoryName { get; init; }

        public string? OwnerName { get; init; }

        public string OwnerType { get; init; } = "User";

        public Guid? OwnerUserId { get; init; }

        public Guid? OwnerTeamId { get; init; }

        public string? Challenge { get; init; }

        public string? Solution { get; init; }

        public string? DurationLabel { get; init; }

        public string? Industry { get; init; }

        public string? TeamLeads { get; init; }

        public string? TestimonialQuote { get; init; }

        public string? TestimonialAuthorName { get; init; }

        public string? TestimonialAuthorTitle { get; init; }

        public string? TestimonialAuthorAvatarUrl { get; init; }

        public PortfolioCreatorDto? Creator { get; init; }

        public IReadOnlyList<OwnerReviewDto> OwnerReviews { get; init; } = [];

        public IEnumerable<PortfolioImageDto> Images { get; init; } = [];

        public IEnumerable<PortfolioSkillDto> Skills { get; init; } = [];

        public IEnumerable<PortfolioRoadmapStepDto> RoadmapSteps { get; init; } = [];

        public IEnumerable<PortfolioMetricDto> Metrics { get; init; } = [];
    }
}
