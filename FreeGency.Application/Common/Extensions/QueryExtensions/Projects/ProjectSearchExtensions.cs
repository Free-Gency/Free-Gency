namespace FreeGency.Application.Common.Extensions.QueryExtensions.Projects
{
    public static class ProjectSearchExtensions
    {
        public static IQueryable<Project> ApplySearch(this IQueryable<Project> query, FilterProjectsRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(p =>
                    p.Title.Contains(request.Search) ||
                    p.Description.Contains(request.Search));
            }

            return query;
        }

        public static IQueryable<Project> ApplySearch(this IQueryable<Project> query, MyProjectsRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                query = query.Where(p =>
                    p.Title.Contains(term) ||
                    p.Description.Contains(term));
            }

            return query;
        }
    }
}
