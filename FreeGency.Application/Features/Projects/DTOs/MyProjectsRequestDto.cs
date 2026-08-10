namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class MyProjectsRequestDto : PagedQuery
    {
        public string Role { get; init; } = "as-client";

        /// <summary>
        /// When Role is as-team, optionally scope to one team (team workspace).
        /// </summary>
        public Guid? TeamId { get; init; }

        /// <summary>
        /// Optional UI filter: draft | open | in-progress | completed | cancelled.
        /// "in-progress" includes both Open and InProgress.
        /// </summary>
        public string? Status { get; init; }

        public string? Search { get; init; }

        public string SortBy { get; init; } = "CreatedAt";

        public string SortDirection { get; init; } = "desc";
    }
}
