namespace FreeGency.Application.Features.Projects.Commands
{
    // Queries
    public partial class ProjectService
    {
        public async Task<ApiResponse<PaginatedResult<ProjectDto>>> BrowseAsync(FilterProjectsRequestDto filterRequest, CancellationToken ct = default)
        {
            var projectsQuery = _projectRepo.GetProjectsQuery();

            var filterdProjects = projectsQuery
                .ApplyFilters(filterRequest)
                .ApplySearch(filterRequest)
                .ApplySorting(filterRequest);

            var pagedResult =
                await PaginatedResult<ProjectDto>.CreateAsync(
                    filterdProjects.ProjectTo<ProjectDto>(_mapper.ConfigurationProvider),
                    filterRequest.PageNumber,
                    filterRequest.PageSize,
                    ct);

            return ApiResponse.Success(pagedResult);
        }

        public async Task<ApiResponse<ProjectDto>> GetDetailsAsync(Guid id, CancellationToken ct = default)
        {
            if (!await _projectRepo.ExistsAsync(id, ct))
                return ApiResponse.Failure<ProjectDto>(AppError.NotFound(nameof(Project), id));

            var project = await _projectRepo.GetByIdWithDetailsAsync(id, ct);

            if (project is null)
                return ApiResponse.Failure<ProjectDto>(AppError.NotFound(nameof(Project), id));

            return ApiResponse.Success(_mapper.Map<ProjectDto>(project));
        }

        public async Task<ApiResponse<PaginatedResult<ProjectDto>>> GetMyProjectsAsync(
            MyProjectsRequestDto request,
            CancellationToken ct = default)
        {
            var filteredProjects = ApplyMyProjectsRoleFilter(_projectRepo.GetProjectsQuery(), request.Role)
                .ApplyFilters(request)
                .ApplySearch(request)
                .ApplySorting(request);

            var pagedResult = await PaginatedResult<ProjectDto>.CreateAsync(
                filteredProjects.ProjectTo<ProjectDto>(_mapper.ConfigurationProvider),
                request.PageNumber,
                request.PageSize,
                ct);

            return ApiResponse.Success(pagedResult);
        }

        public async Task<ApiResponse<MyProjectsSummaryDto>> GetMyProjectsSummaryAsync(
            string role,
            CancellationToken ct = default)
        {
            var query = ApplyMyProjectsRoleFilter(_projectRepo.GetProjectsQuery(), role);

            var counts = await query
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            int CountOf(ProjectStatus status) =>
                counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

            var draft = CountOf(ProjectStatus.Draft);
            var open = CountOf(ProjectStatus.Open);
            var inProgress = CountOf(ProjectStatus.InProgress);
            var completed = CountOf(ProjectStatus.Completed);
            var cancelled = CountOf(ProjectStatus.Cancelled);

            var upcomingDeadlines = await query
                .Where(p => p.Deadline != null)
                .OrderBy(p => p.Deadline)
                .Take(4)
                .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return ApiResponse.Success(new MyProjectsSummaryDto
            {
                Total = draft + open + inProgress + completed + cancelled,
                Draft = draft,
                Open = open,
                InProgress = inProgress,
                Completed = completed,
                Cancelled = cancelled,
                UpcomingDeadlines = upcomingDeadlines,
            });
        }

        public async Task<ApiResponse<IEnumerable<ProjectDto>>> GetSavedProjectsAsync(CancellationToken ct = default)
            => ApiResponse.Success(
                _mapper.Map<IEnumerable<ProjectDto>>(
                    await _projectRepo.GetSavedByUserAsync(_currentUser.UserId, ct)));

        private IQueryable<Project> ApplyMyProjectsRoleFilter(IQueryable<Project> query, string role)
        {
            var userId = _currentUser.UserId;
            var normalized = role?.Trim().ToLowerInvariant();

            return normalized switch
            {
                "as-client" => query.Where(p => p.ClientId == userId),
                "as-assignee" => query.Where(p =>
                    p.AssignedUserId == userId ||
                    (p.AssignedTeamId != null &&
                     _teamMemberRepo.Query().Any(tm => tm.TeamId == p.AssignedTeamId && tm.UserId == userId))),
                _ => throw new AppValidationException(
                    "Role",
                    "Role must be either 'as-client' or 'as-assignee'."),
            };
        }
    }
}
