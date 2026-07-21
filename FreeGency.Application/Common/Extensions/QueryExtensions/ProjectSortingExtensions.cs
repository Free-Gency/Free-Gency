using System.Linq;
using FreeGency.Application.Features.Projects.DTOs;

namespace FreeGency.Application.Common.Extensions.QueryExtensions
{
    public static class ProjectSortingExtensions
    {
        public static IQueryable<ProjectDto> ApplyFilters(this IQueryable<ProjectDto> query)
            => query;
    }
}
