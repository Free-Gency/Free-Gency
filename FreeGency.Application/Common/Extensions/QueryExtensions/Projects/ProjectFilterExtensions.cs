namespace FreeGency.Application.Common.Extensions.QueryExtensions.Projects
{
    public static class ProjectFilterExtensions
    {
        public static IQueryable<Project> ApplyFilters(this IQueryable<Project> query, FilterProjectsRequestDto request)
        {
            if (request.Status.HasValue)
                query = query.Where(i => i.Status == request.Status);

            if (request.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == request.CategoryId);

            if (request.SpecialtyId.HasValue)
                query = query.Where(i => i.ProjectSpecialties.Any(s => s.Id == request.SpecialtyId));

            if (request.BudgetMin.HasValue)
                query = query.Where(p => p.BudgetMax >= request.BudgetMin.Value);

            if (request.BudgetMax.HasValue)
                query = query.Where(p => p.BudgetMin <= request.BudgetMax.Value);

            if (request.IsFixedPrice.HasValue)
                query = query.Where(i => i.IsFixedPrice);

            if (!string.IsNullOrWhiteSpace(request.Currency))
                query = query.Where(i => i.Currency == request.Currency);


            return query;
        }

        public static IQueryable<Project> ApplyFilters(this IQueryable<Project> query, MyProjectsRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return query;

            return request.Status.Trim().ToLowerInvariant() switch
            {
                "draft" => query.Where(p => p.Status == ProjectStatus.Draft),
                "open" => query.Where(p => p.Status == ProjectStatus.Open),
                "in-progress" => query.Where(p =>
                    p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Open),
                "completed" => query.Where(p => p.Status == ProjectStatus.Completed),
                "cancelled" => query.Where(p => p.Status == ProjectStatus.Cancelled),
                _ => query,
            };
        }
    }
}
