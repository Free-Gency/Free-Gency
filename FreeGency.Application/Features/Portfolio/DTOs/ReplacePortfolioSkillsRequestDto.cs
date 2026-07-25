namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class ReplacePortfolioSkillsRequestDto
    {
        public IEnumerable<Guid> SkillIds { get; init; } = [];
    }
}
