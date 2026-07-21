namespace FreeGency.Application.Features.Projects.DTOs
{
    public sealed class BrowseProjectsDto : PagedQuery
    {
        public bool? IsFixedPrice { get; init; }
        public decimal? BudgetMin { get; init; }
        public decimal? BudgetMax { get; init; }
        public Guid? CategoryId { get; init; }
        public Guid? SpecialtyId { get; init; }
        public string? Currency { get; init; }
        public ProjectStatus? Status { get; init; }
        public string? Search { get; init; }
        public string SortBy { get; init; } = "CreatedAt";
        public string SortDirection { get; init; } = "desc";
    }
}
