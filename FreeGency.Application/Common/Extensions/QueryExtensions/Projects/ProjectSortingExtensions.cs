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

                "deadline" =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.Deadline == null).ThenBy(p => p.Deadline)
                        : query.OrderByDescending(p => p.Deadline == null).ThenByDescending(p => p.Deadline),

                "proposalcount" =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.ProjectProposals.Count())
                        : query.OrderByDescending(p => p.ProjectProposals.Count()),

                _ =>
                    request.SortDirection == "asc"
                        ? query.OrderBy(p => p.CreatedAt)
                        : query.OrderByDescending(p => p.CreatedAt)
            };

            return query;
        }
    }
}
