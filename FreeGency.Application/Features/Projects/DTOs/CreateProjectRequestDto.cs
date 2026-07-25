namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class CreateProjectRequestDto
    {
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public Guid CategoryId { get; init; }
        public bool IsFixedPrice { get; init; }
        public decimal BudgetMin { get; init; }
        public decimal BudgetMax { get; init; }
        public string Currency { get; init; } = "USD";
        public int? EstimatedDurationDays { get; init; }
        public IEnumerable<Guid> SkillIds { get; init; } = [];
        public IEnumerable<Guid> SpecialtyIds { get; init; } = [];
    }
}
