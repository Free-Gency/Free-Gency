using Microsoft.AspNetCore.Http;

namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class CreatePortfolioProjectRequestDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public owner OwnerType { get; set; }

        public Guid? OwnerTeamId { get; set; }

        public decimal? Budget { get; set; }

        public string? ProjectUrl { get; set; }

        public string? PrototypeUrl { get; set; }

        public DateTime? CompletionDate { get; set; }

        public Guid? CategoryId { get; set; }

        public Visibility Visibility { get; set; } = Visibility.Public;

        public string? Challenge { get; set; }

        public string? Solution { get; set; }

        public string? DurationLabel { get; set; }

        public string? Industry { get; set; }

        public string? TeamLeads { get; set; }

        public string? TestimonialQuote { get; set; }

        public string? TestimonialAuthorName { get; set; }

        public string? TestimonialAuthorTitle { get; set; }

        public string? TestimonialAuthorAvatarUrl { get; set; }

        public List<Guid> SkillIds { get; set; } = [];

        public List<PortfolioRoadmapStepDto> RoadmapSteps { get; set; } = [];

        public List<PortfolioMetricDto> Metrics { get; set; } = [];

        public List<IFormFile>? Images { get; set; }
    }
}
