namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class UpdateProjectRequestDto
    {
        public Guid Id { get; init; }

        public string? Title { get; init; }

        public string? Description { get; init; }

        public Guid? CategoryId { get; init; }

        public bool? IsFixedPrice { get; init; }

        public decimal? BudgetMin { get; init; }

        public decimal? BudgetMax { get; init; }

        public string? Currency { get; init; }

        public int? EstimatedDurationDays { get; init; }

        public List<Guid>? SpecialtyIds { get; init; }

        public List<Guid>? SkillIds { get; init; }
    }
}
