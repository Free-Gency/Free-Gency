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

        public async Task<ApiResponse<IEnumerable<ProjectDto>>> GetMyProjectsAsync(string role, CancellationToken ct = default)
        {
            var userId = _currentUser.UserId;

            IEnumerable<Project> projects = role.ToLower() switch
            {
                "as-client" =>
                    await _projectRepo.GetMineAsync(userId, true, ct),

                "as-assignee" =>
                    await _projectRepo.GetMineAsync(userId, false, ct),

                _ => throw new AppValidationException("Role", "Role must be either 'as-client' or 'as-assignee'.")
            };

            var result = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            return ApiResponse.Success(result);
        }

        public async Task<ApiResponse<IEnumerable<ProjectDto>>> GetSavedProjectsAsync(CancellationToken ct = default)
            => ApiResponse.Success(
                _mapper.Map<IEnumerable<ProjectDto>>(
                    await _projectRepo.GetSavedByUserAsync(_currentUser.UserId, ct)));

    }
}
