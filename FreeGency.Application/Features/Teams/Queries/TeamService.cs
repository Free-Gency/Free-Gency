

namespace FreeGency.Application.Features.Teams.Commands;

public partial class TeamService
{
    public async Task<Result<WalletTeamDto>> GetTeamWallet(Guid teamid)
    { 
        var wallet = await _walletRepository.GetByOwnerAsync(owner.Team, teamid);
        if (wallet == null) return Result.Failure<WalletTeamDto>(WalletErrors.NotFound);
        if (wallet.OwnerTeamId == null)
            return Result.Failure<WalletTeamDto>(WalletErrors.NotFound);
        var TotalEarning = await _ledgerEntryRepository.GetTotalEarningsAsync(wallet.Id);
        var WalletTeamDto = new WalletTeamDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Pending = wallet.Pending,
            Available = wallet.Available,
            Reserved = wallet.Reserved,
            TeamId=wallet.OwnerTeamId.Value,
            TotalEarnings=TotalEarning
        };
        return Result.Success(WalletTeamDto);
    }
    public async Task<Result<PaginatedResult<TeamProjectEarningsDto>>>
  GetTeamProjectEarnings(TeamProjectsFilter teamProjectsFilter)
    {
        var teamProjects = _teamRepository
            .GetProjectTeamAccepted(teamProjectsFilter.TeamId)
            .Include(p => p.MilestonePlanVersions)
                .ThenInclude(v => v.Items)
            .Include(p => p.Milestones);

        var page = await PaginatedResult<Project>.CreateAsync(
            teamProjects,
            teamProjectsFilter.PageNumber,
            teamProjectsFilter.PageSize);

        var items = new List<TeamProjectEarningsDto>();
        foreach (var project in page.Items)
        {
            var totalBudget = project.MilestonePlanVersions
                .Where(v => v.Status == PlanVersionStatus.Accepted)
                .SelectMany(v => v.Items)
                .Sum(x => x.Amount);

            var releasedAmount = project.Milestones.Sum(x => x.ReleasedAmount);

            var splits = project.AssignedTeamId is Guid teamId
                ? (await _payoutSplitRepository.GetByProjectAsync(teamId, project.Id)).ToList()
                : [];

            // Prefer project-level rows when present; otherwise average milestone % per user.
            var projectLevel = splits.Where(s => s.MilestoneId is null).ToList();
            var source = projectLevel.Count > 0
                ? projectLevel
                : splits.Where(s => s.MilestoneId is not null).ToList();

            var members = source
                .GroupBy(s => s.UserId)
                .Select(g =>
                {
                    var pct = g.Average(x => x.Value);
                    var user = g.First().User;
                    var name = user is null
                        ? string.Empty
                        : $"{user.FristName} {user.LastName}".Trim();
                    return new TeamMemberEarningDto
                    {
                        UserId = g.Key,
                        Name = name,
                        Role = null,
                        Percentage = Math.Round(pct, 2, MidpointRounding.AwayFromZero),
                        Amount = totalBudget * pct / 100m,
                        ReleasedAmount = releasedAmount * pct / 100m,
                        Status = releasedAmount == 0
                            ? "Pending"
                            : releasedAmount >= totalBudget && totalBudget > 0
                                ? "Released"
                                : "Partially Released"
                    };
                })
                .OrderByDescending(m => m.Percentage)
                .ToList();

            items.Add(new TeamProjectEarningsDto
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                Currency = project.Currency,
                TotalBudget = totalBudget,
                ReleasedAmount = releasedAmount,
                Members = members
            });
        }

        return Result.Success(PaginatedResult<TeamProjectEarningsDto>.FromList(
            items,
            page.PageNumber,
            page.PageSize,
            page.TotalCount));
    }
    public async Task<ApiResponse<PaginatedResult<TeamDto>>> BrowseAsync(
        FilterTeamsRequestDto filter,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var (items, totalCount) = await _teamRepository.GetBrowseHubItemsPagedAsync(
            userId == Guid.Empty ? null : userId,
            filter.Search,
            filter.CategoryId,
            filter.ExcludeMine,
            filter.PageNumber,
            filter.PageSize,
            ct);

        var page = PaginatedResult<TeamDto>.FromList(
            items.Select(t => t.ToDto()).ToList(),
            filter.PageNumber,
            filter.PageSize,
            totalCount);

        return ApiResponse.Success(page);
    }

    public async Task<ApiResponse<TeamDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdWithDetailsAsync(id, ct);
        if (team is null)
            return ApiResponse.Failure<TeamDto>(AppError.NotFound(nameof(Team), id));

        var userId = _currentUserService.UserId;
        return ApiResponse.Success(team.ToDto(userId == Guid.Empty ? null : userId));
    }

    public async Task<ApiResponse<IEnumerable<TeamDto>>> GetMineAsync(CancellationToken ct = default)
    {
        var teams = await _teamRepository.GetMyHubItemsAsync(_currentUserService.UserId, ct);
        return ApiResponse.Success(teams.Select(t => t.ToDto()));
    }

    public async Task<ApiResponse<TeamDto>> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByTeamCodeWithDetailsAsync(teamCode, ct);
        if (team is null)
            return ApiResponse.Failure<TeamDto>(AppError.NotFound(nameof(Team), teamCode));

        var userId = _currentUserService.UserId;
        return ApiResponse.Success(team.ToDto(userId == Guid.Empty ? null : userId));
    }

    public async Task<ApiResponse<IReadOnlyList<TeamReviewDto>>> GetReviewsAsync(
        Guid teamId,
        CancellationToken ct = default)
    {
        if (!await _teamRepository.ExistsAsync(teamId, ct))
            return ApiResponse.Failure<IReadOnlyList<TeamReviewDto>>(AppError.NotFound(nameof(Team), teamId));

        var feedback = await _teamRepository.GetFeedbackAsync(teamId, 40, ct);
        return ApiResponse.Success<IReadOnlyList<TeamReviewDto>>(feedback.Select(MapTeamReview).ToList());
    }



    #region Team Projects

    public async Task<ApiResponse<IEnumerable<TeamProjectCardDto>>> GetTeamProjectsAsync(
        Guid teamId, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<IEnumerable<TeamProjectCardDto>>(AppError.NotFound(nameof(Team), teamId));

        var userId = _currentUserService.UserId;
        if (!await _teamMemberRepository.IsMemberAsync(teamId, userId, ct))
            return ApiResponse.Failure<IEnumerable<TeamProjectCardDto>>(
                AppError.Forbidden("You are not a member of this team."));

        var isLeader = await _teamMemberRepository.IsLeaderAsync(teamId, userId, ct);

        var teamProjects = await _projectRepository.GetProjectsQuery()
            .Where(p => p.AssignedTeamId == teamId)
            .Include(p => p.Client)
            .Include(p => p.Category)
            .ToListAsync(ct);


        var visible = isLeader
            ? teamProjects
            : await FilterMemberProjectsAsync(teamProjects, userId, ct);

        var projectIds = visible.Select(p => p.Id).ToList();
        var milestones = await _milestoneRepository.Query()
            .Where(m => projectIds.Contains(m.ProjectId))
            .ToListAsync(ct);

        var counts = milestones
            .GroupBy(m => m.ProjectId)
            .ToDictionary(g => g.Key,
                g => (Total: g.Count(), Done: g.Count(m => m.WorkStatus == WorkStatus.Approved)));

        var myProjectIds = (await _projectMemberRepository.GetByUserIdAsync(userId, ct))
            .Select(pm => pm.ProjectId)
            .ToHashSet();

        var dtos = visible
            .Select(p =>
            {
                var (total, done) = counts.GetValueOrDefault(p.Id);
                var percent = total > 0 ? (int)Math.Round(done * 100.0 / total) : 0;
                return new TeamProjectCardDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status.ToString(),
                    ClientName = FullName(p.Client),
                    BudgetMin = p.BudgetMin,
                    BudgetMax = p.BudgetMax,
                    Currency = p.Currency,
                    Deadline = p.Deadline,
                    CategoryName = p.Category?.Name,
                    TotalMilestones = total,
                    CompletedMilestones = done,
                    ProgressPercent = percent,
                    IsCurrentUserMember = p.AssignedUserId == userId || myProjectIds.Contains(p.Id)
                };
            })
            .OrderByDescending(d => d.Deadline ?? DateTime.MaxValue)
            .ToList();

        return ApiResponse.Success<IEnumerable<TeamProjectCardDto>>(dtos);
    }

    public async Task<ApiResponse<IEnumerable<ProjectMemberDto>>> GetProjectMembersAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<ProjectMemberDto>>(AppError.NotFound(nameof(Project), projectId));

        if (!await CanAccessProjectAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<ProjectMemberDto>>(
                AppError.Forbidden("You do not have access to this project."));

        var members = await _projectMemberRepository.GetMembersWithUsersAsync(projectId, ct);
        var dtos = members.Select(m => new ProjectMemberDto
        {
            UserId = m.UserId,
            Name = FullName(m.User),
            ImageUrl = m.User?.DeveloperProfile?.ProfileImage,
            RoleInProject = m.RoleInProject ?? "Member",
            AssignedAt = m.AssignedAt
        });

        return ApiResponse.Success<IEnumerable<ProjectMemberDto>>(dtos);
    }

    public async Task<ApiResponse<ProjectMemberDto>> AssignProjectMemberAsync(
        Guid projectId, AssignProjectMemberDto dto, CancellationToken ct = default)
    {
        var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
        if (profileError is not null)
            return ApiResponse.Failure<ProjectMemberDto>(profileError);

        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<ProjectMemberDto>(AppError.NotFound(nameof(Project), projectId));

        if (!await CanManageTeamProjectAsync(project, ct))
            return ApiResponse.Failure<ProjectMemberDto>(
                AppError.Forbidden("Only the team leader can assign members to this project."));

        if (project.AssignedTeamId is null)
            return ApiResponse.Failure<ProjectMemberDto>(
                AppError.Validation("This project is not assigned to a team."));

        if (!await _teamMemberRepository.IsMemberAsync(project.AssignedTeamId.Value, dto.UserId, ct))
            return ApiResponse.Failure<ProjectMemberDto>(
                AppError.Validation("The user is not a member of the assigned team."));

        await _projectMemberRepository.AddMemberAsync(projectId, dto.UserId, dto.RoleInProject,
            _currentUserService.UserId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var rows = await _projectMemberRepository.GetMembersWithUsersAsync(projectId, ct);
        var row = rows.FirstOrDefault(m => m.UserId == dto.UserId);
        return ApiResponse.Success(new ProjectMemberDto
        {
            UserId = row!.UserId,
            Name = FullName(row.User),
            ImageUrl = row.User?.DeveloperProfile?.ProfileImage,
            RoleInProject = row.RoleInProject ?? "Member",
            AssignedAt = row.AssignedAt
        });
    }

    public async Task<ApiResponse> RemoveProjectMemberAsync(
        Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), projectId));

        if (!await CanManageTeamProjectAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the team leader can remove members."));

        await _projectMemberRepository.RemoveAsync(projectId, userId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Member removed from project.");
    }

    public async Task<ApiResponse<IEnumerable<MilestoneAssignmentDto>>> GetMilestoneAssignmentsAsync(
        Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<IEnumerable<MilestoneAssignmentDto>>(
                AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepository.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<MilestoneAssignmentDto>>(
                AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await CanAccessProjectAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<MilestoneAssignmentDto>>(
                AppError.Forbidden("You do not have access to this milestone."));

        var rows = await _milestoneAssignmentRepository.GetByMilestoneIdAsync(milestoneId, ct);
        var dtos = rows.Select(a => new MilestoneAssignmentDto
        {
            Id = a.Id,
            MilestoneId = a.MilestoneId,
            UserId = a.UserId,
            UserName = FullName(a.User),
            ImageUrl = a.User?.DeveloperProfile?.ProfileImage,
            Percentage = a.Percentage
        });

        return ApiResponse.Success<IEnumerable<MilestoneAssignmentDto>>(dtos);
    }

    public async Task<ApiResponse> SetMilestoneAssignmentsAsync(
        Guid milestoneId, SetMilestoneAssignmentsDto dto, CancellationToken ct = default)
    {
        var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        var milestone = await _milestoneRepository.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepository.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await CanManageTeamProjectAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the team leader can set milestone splits."));

        var items = dto?.Items ?? [];
        if (items.Any(i => i.Percentage < 0))
            return ApiResponse.Failure(AppError.Validation("Percentages cannot be negative."));
        if (items.Sum(i => i.Percentage) > 100)
            return ApiResponse.Failure(AppError.Validation("The sum of percentages cannot exceed 100."));

        var assignedBy = _currentUserService.UserId;
        var now = DateTime.UtcNow;


        await _milestoneAssignmentRepository.RemoveForMilestoneAsync(milestoneId, ct);

        foreach (var item in items)
        {
            if (item.Percentage == 0)
                continue;
            if (project.AssignedTeamId is not { } teamId ||
                !await _teamMemberRepository.IsMemberAsync(teamId, item.UserId, ct))
                return ApiResponse.Failure(
                    AppError.Validation("A member does not belong to the assigned team."));

            await _milestoneAssignmentRepository.AddAsync(new MilestoneAssignment
            {
                Id = Guid.NewGuid(),
                MilestoneId = milestoneId,
                UserId = item.UserId,
                Percentage = item.Percentage,
                AssignedByUserId = assignedBy,
                AssignedAt = now,
                CreatedAt = now,
                CreatedBy = assignedBy.ToString()
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Milestone assignments saved.");
    }

    public async Task<ApiResponse<IEnumerable<MilestoneAssigneeDto>>> GetMilestoneAssigneesAsync(
        Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<IEnumerable<MilestoneAssigneeDto>>(
                AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepository.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<MilestoneAssigneeDto>>(
                AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await CanManageTeamProjectAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<MilestoneAssigneeDto>>(
                AppError.Forbidden("Only the team leader can view assignee options."));

        var assignees = new List<MilestoneAssigneeDto>();

        if (project.AssignedTeamId.HasValue)
        {
            var teamId = project.AssignedTeamId.Value;
            var milestoneSplits = (await _payoutSplitRepository.GetByScopeAsync(
                teamId, project.Id, milestoneId, ct)).ToList();

            var eligibleUserIds = milestoneSplits.Count > 0
                ? milestoneSplits.Select(a => a.UserId).ToList()
                : (await _projectMemberRepository.GetByProjectIdAsync(project.Id, ct))
                    .Select(m => m.UserId).ToList();

            if (eligibleUserIds.Count > 0)
            {
                var users = await _userRepository.Query()
                    .Include(u => u.DeveloperProfile)
                    .Where(u => eligibleUserIds.Contains(u.Id))
                    .ToListAsync(ct);

                assignees = users.Select(u => new MilestoneAssigneeDto
                {
                    UserId = u.Id,
                    Name = FullName(u),
                    ImageUrl = u.DeveloperProfile?.ProfileImage
                }).ToList();
            }
        }
        else if (project.AssignedUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(project.AssignedUserId.Value, ct);
            assignees.Add(new MilestoneAssigneeDto
            {
                UserId = project.AssignedUserId.Value,
                Name = FullName(user),
                ImageUrl = user?.DeveloperProfile?.ProfileImage
            });
        }

        return ApiResponse.Success<IEnumerable<MilestoneAssigneeDto>>(assignees);
    }

    private async Task<List<Project>> FilterMemberProjectsAsync(
        List<Project> teamProjects, Guid userId, CancellationToken ct)
    {
        var myProjectIds = (await _projectMemberRepository.GetByUserIdAsync(userId, ct))
            .Select(pm => pm.ProjectId)
            .ToHashSet();
        return teamProjects
            .Where(p => p.AssignedUserId == userId || myProjectIds.Contains(p.Id))
            .ToList();
    }

    private async Task<bool> CanAccessProjectAsync(Project project, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (project.ClientId == userId || project.AssignedUserId == userId)
            return true;
        return project.AssignedTeamId is not null &&
               await _teamMemberRepository.IsMemberAsync(project.AssignedTeamId.Value, userId, ct);
    }

    private async Task<bool> CanManageTeamProjectAsync(Project project, CancellationToken ct)
    {
        if (project.AssignedTeamId is null)
            return false;
        return await _teamMemberRepository.IsLeaderAsync(
            project.AssignedTeamId.Value, _currentUserService.UserId, ct);
    }

    private async Task<AppError?> RequireActiveProfileModeAsync(profileMode required, CancellationToken ct)
    {
        var active = await _userRepository.GetActiveProfileAsync(_currentUserService.UserId, ct);
        if (active is null)
            return AppError.Validation(
                "An active profile is required. Create or switch to a Client or Developer profile.");

        if (active.Value.Mode != required)
            return AppError.Forbidden(required == profileMode.Client
                ? "Switch to Client profile to perform this action."
                : "Switch to Developer profile to perform this action.");

        return null;
    }

    private static string FullName(User? user)
        => user is null ? string.Empty : $"{user.FristName} {user.LastName}".Trim();

    #endregion

}
