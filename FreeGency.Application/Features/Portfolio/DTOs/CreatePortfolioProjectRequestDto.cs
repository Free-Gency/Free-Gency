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

        public DateTime? CompletionDate { get; init; }

        public Guid? CategoryId { get; init; }

        public Visibility Visibility { get; init; } = Visibility.Public;

        public IEnumerable<Guid> SkillIds { get; init; } = [];

        public IEnumerable<IFormFile>? Images { get; init; }
    }
}
