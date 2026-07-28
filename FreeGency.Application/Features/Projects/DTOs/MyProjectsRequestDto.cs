namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class MyProjectsRequestDto : PagedQuery
    {
        public string Role { get; init; } = "as-client";

        /// <summary>
        /// Optional UI filter: draft | open | in-progress | completed | cancelled.
        /// "in-progress" includes both Open and InProgress.
        /// </summary>
        public string? Status { get; init; }

        public string? Search { get; init; }
    }
}
