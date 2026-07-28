namespace FreeGency.Application.Common.Extensions.QueryExtensions.Projects
{
    public static class ProjectSortingExtensions
    {
        public static IQueryable<Project> ApplySorting(this IQueryable<Project> query, FilterProjectsRequestDto request)
        {
            query = request.SortBy.ToLower() switch
            {
                "title" =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.Title)
                        : query.OrderByDescending(p => p.Title),

                "budget" =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.BudgetMin)
                        : query.OrderByDescending(p => p.BudgetMin),

                _ =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.CreatedAt)
                        : query.OrderByDescending(p => p.CreatedAt)
            };

            return query;
        }

        public static IQueryable<Project> ApplySorting(this IQueryable<Project> query, MyProjectsRequestDto request)
        {
            query = request.SortBy.ToLower() switch
            {
                "title" =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.Title)
                        : query.OrderByDescending(p => p.Title),

                "budget" =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.BudgetMin)
                        : query.OrderByDescending(p => p.BudgetMin),

                _ =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.CreatedAt)
                        : query.OrderByDescending(p => p.CreatedAt)
            };

            return query;
        }
    }
}
