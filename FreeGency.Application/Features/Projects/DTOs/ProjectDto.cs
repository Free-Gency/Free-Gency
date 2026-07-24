namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class ProjectDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public bool IsFixedPrice { get; init; }

        public decimal BudgetMin { get; init; }

        public decimal BudgetMax { get; init; }

        public string Currency { get; init; } = string.Empty;

        public DateTime? Deadline { get; init; }

        public int? EstimatedDurationDays { get; init; }

        public string Status { get; init; } = default!;

        public DateTime CreatedAt { get; init; }

        public string CategoryName { get; init; } = string.Empty;

        public string ClientName { get; init; } = string.Empty;

        public string? ClientAvatarUrl { get; init; }

        public IEnumerable<string> Specialties { get; init; } = [];

        public IEnumerable<string> Skills { get; init; } = [];

        public int ProposalCount { get; init; }

        //public bool IsSaved { get; init; }
    }
}
