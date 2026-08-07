using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class CreatePortfolioProjectRequestDto
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public owner OwnerType { get; init; }

        public Guid? OwnerTeamId { get; init; }

        public decimal? Budget { get; init; }

        public string? ProjectUrl { get; init; }

        public string? PrototypeUrl { get; init; }

        public DateTime? CompletionDate { get; init; }

        public Guid? CategoryId { get; init; }

        public Visibility Visibility { get; init; } = Visibility.Public;

        public string? Challenge { get; init; }

        public string? Solution { get; init; }

        public string? DurationLabel { get; init; }

        public string? Industry { get; init; }

        public string? TeamLeads { get; init; }

        public string? TestimonialQuote { get; init; }

        public string? TestimonialAuthorName { get; init; }

        public string? TestimonialAuthorTitle { get; init; }

        public string? TestimonialAuthorAvatarUrl { get; init; }

        public IEnumerable<Guid> SkillIds { get; init; } = [];

        public List<PortfolioRoadmapStepDto> RoadmapSteps { get; init; } = [];

        public List<PortfolioMetricDto> Metrics { get; init; } = [];

        public IEnumerable<IFormFile>? Images { get; init; }
    }
}
