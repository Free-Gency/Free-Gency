namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class ProjectDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public Guid? ClientId { get; init; }

        public bool IsFixedPrice { get; init; }

        public decimal BudgetMin { get; init; }

        public decimal BudgetMax { get; init; }

        public string Currency { get; init; } = string.Empty;

        public DateTime? Deadline { get; init; }

        public int? EstimatedDurationDays { get; init; }

        public string Status { get; init; } = default!;

        public Guid? AssignedTeamId { get; init; }

        public Guid? AssignedUserId { get; init; }

        public DateTime CreatedAt { get; init; }

        public string CategoryName { get; init; } = string.Empty;

        public string? CategoryId { get; init; }

        public string ClientName { get; init; } = string.Empty;

        public string? ClientAvatarUrl { get; init; }

        public decimal? ClientRating { get; init; }

        public IEnumerable<string> Specialties { get; init; } = [];

        public IEnumerable<string> Skills { get; init; } = [];

        public IEnumerable<string> SkillIds { get; init; } = [];

        public int ProposalCount { get; init; }

        /// <summary>True when at least one proposal is InDiscussion on this project (informational; multiple allowed).</summary>
        public bool HasActiveDiscussion { get; init; }
    }
}