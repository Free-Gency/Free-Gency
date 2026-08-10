using FreeGency.Application.Features.PayoutSplits.DTOs;
using FreeGency.Domain.Interfaces.Repositories.Teams;

namespace FreeGency.Application.Features.PayoutSplits.Commands;

public sealed class PayoutSplitService : IPayoutSplitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ITeamRepository _teamRepo;
    private readonly ITeamMemberRepository _teamMemberRepo;
    private readonly ITeamPayoutSplitRepository _splitRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IMilestoneRepository _milestoneRepo;
    private readonly IProjectMemberRepository _projectMemberRepo;
    private readonly IUserRepository _userRepo;

    public PayoutSplitService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _teamRepo = _unitOfWork.Repository<ITeamRepository, Team>();
        _teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _splitRepo = _unitOfWork.Repository<ITeamPayoutSplitRepository, TeamPayoutSplit>();
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
        _milestoneRepo = _unitOfWork.Repository<IMilestoneRepository, Milestone>();
        _projectMemberRepo = _unitOfWork.Repository<IProjectMemberRepository, ProjectMember>();
        _userRepo = _unitOfWork.Repository<IUserRepository, User>();
    }

    public async Task<ApiResponse<PayoutSplitsDto>> GetTeamDefaultsAsync(Guid teamId, CancellationToken ct = default)
    {
        var team = await _teamRepo.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Team), teamId));

        if (!await CanManageTeamAsync(team, ct))
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.Forbidden("Only the team owner or leader can view payout splits."));

        var splits = (await _splitRepo.GetByScopeAsync(teamId, null, null, ct)).ToList();
        return ApiResponse.Success(ToDto(teamId, null, null, splits));
    }

    public async Task<ApiResponse<PayoutSplitsDto>> ReplaceTeamDefaultsAsync(
        Guid teamId,
        ReplacePayoutSplitsDto dto,
        CancellationToken ct = default)
    {
        var team = await _teamRepo.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Team), teamId));

        var authError = await EnsureLeaderDeveloperAsync(team, ct);
        if (authError is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(authError);

        if (!TryParseSplitType(dto.SplitType, out var splitType))
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.Validation("SplitType must be Percent or Fixed."));

        // Team defaults: Percent only (Fixed needs a concrete milestone/project total).
        if (splitType != Domain.Enums.SplitType.Percent)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Team default splits must use Percent. Use project overrides for Fixed amounts."));

        var buildError = await BuildSplitsAsync(teamId, null, null, splitType, dto.Items, totalAmount: 100m, ct);
        if (buildError.Error is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(buildError.Error);

        await _splitRepo.ReplaceSplitsAsync(teamId, null, null, buildError.Splits!, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = (await _splitRepo.GetByScopeAsync(teamId, null, null, ct)).ToList();
        return ApiResponse.Success(ToDto(teamId, null, null, saved), "Team payout splits updated.");
    }

    public async Task<ApiResponse<PayoutSplitsDto>> GetProjectSplitsAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Project), projectId));

        if (project.AssignedTeamId is null)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Payout splits apply only to team-assigned projects."));

        var team = await _teamRepo.GetByIdAsync(project.AssignedTeamId.Value, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Team), project.AssignedTeamId.Value));

        if (!await CanManageTeamAsync(team, ct))
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.Forbidden("Only the team owner or leader can view payout splits."));

        var projectSplits = (await _splitRepo.GetByScopeAsync(team.Id, projectId, null, ct)).ToList();
        if (projectSplits.Count > 0)
            return ApiResponse.Success(ToDto(team.Id, projectId, null, projectSplits));

        // Fallback: team defaults
        var defaults = (await _splitRepo.GetByScopeAsync(team.Id, null, null, ct)).ToList();
        return ApiResponse.Success(ToDto(team.Id, projectId: null, null, defaults));
    }

    public async Task<ApiResponse<PayoutSplitsDto>> ReplaceProjectSplitsAsync(
        Guid projectId,
        ReplacePayoutSplitsDto dto,
        CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Project), projectId));

        if (project.AssignedTeamId is null)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Payout splits apply only to team-assigned projects."));

        var team = await _teamRepo.GetByIdAsync(project.AssignedTeamId.Value, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Team), project.AssignedTeamId.Value));

        var authError = await EnsureLeaderDeveloperAsync(team, ct);
        if (authError is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(authError);

        if (!TryParseSplitType(dto.SplitType, out var splitType))
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.Validation("SplitType must be Percent or Fixed."));

        var totalForValidation = splitType == Domain.Enums.SplitType.Percent
            ? 100m
            : (project.IsFixedPrice
                ? (project.BudgetMax > 0 ? project.BudgetMax : project.BudgetMin)
                : project.BudgetMax);

        if (splitType == Domain.Enums.SplitType.Fixed && totalForValidation <= 0)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Project budget is required to validate Fixed payout splits."));

        var buildError = await BuildSplitsAsync(
            team.Id,
            projectId,
            null,
            splitType,
            dto.Items,
            totalForValidation,
            ct);
        if (buildError.Error is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(buildError.Error);

        await _splitRepo.ReplaceSplitsAsync(team.Id, projectId, null, buildError.Splits!, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = (await _splitRepo.GetByScopeAsync(team.Id, projectId, null, ct)).ToList();
        return ApiResponse.Success(ToDto(team.Id, projectId, null, saved), "Project payout splits updated.");
    }

    public async Task<ApiResponse<PayoutSplitsDto>> GetMilestoneSplitsAsync(
        Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (project.AssignedTeamId is null)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Payout splits apply only to team-assigned projects."));

        var team = await _teamRepo.GetByIdAsync(project.AssignedTeamId.Value, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.NotFound(nameof(Team), project.AssignedTeamId.Value));

        if (!await CanManageTeamAsync(team, ct))
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Forbidden("Only the team owner or leader can view payout splits."));

        var splits = (await _splitRepo.GetByScopeAsync(team.Id, project.Id, milestoneId, ct)).ToList();
        return ApiResponse.Success(ToDto(team.Id, project.Id, milestoneId, splits));
    }

    public async Task<ApiResponse<PayoutSplitsDto>> ReplaceMilestoneSplitsAsync(
        Guid milestoneId,
        ReplacePayoutSplitsDto dto,
        CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (project.AssignedTeamId is null)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Payout splits apply only to team-assigned projects."));

        var team = await _teamRepo.GetByIdAsync(project.AssignedTeamId.Value, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.NotFound(nameof(Team), project.AssignedTeamId.Value));

        var authError = await EnsureLeaderDeveloperAsync(team, ct);
        if (authError is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(authError);

        if (!TryParseSplitType(dto.SplitType, out var splitType) || splitType != Domain.Enums.SplitType.Percent)
            return ApiResponse.Failure<PayoutSplitsDto>(
                AppError.Validation("Milestone payout splits must use Percent and sum to 100."));

        var buildError = await BuildSplitsAsync(
            team.Id,
            project.Id,
            milestoneId,
            splitType,
            dto.Items,
            totalAmount: 100m,
            ct,
            preferProjectMembers: true);
        if (buildError.Error is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(buildError.Error);

        await _splitRepo.ReplaceSplitsAsync(team.Id, project.Id, milestoneId, buildError.Splits!, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = (await _splitRepo.GetByScopeAsync(team.Id, project.Id, milestoneId, ct)).ToList();
        return ApiResponse.Success(
            ToDto(team.Id, project.Id, milestoneId, saved),
            "Milestone payout splits updated.");
    }

    private async Task<AppError?> EnsureLeaderDeveloperAsync(Team team, CancellationToken ct)
    {
        if (!await CanManageTeamAsync(team, ct))
            return AppError.Forbidden("Only the team owner or leader can manage payout splits.");

        var active = await _userRepo.GetActiveProfileAsync(_currentUser.UserId, ct);
        if (active is null)
            return AppError.Validation("An active profile is required.");

        if (active.Value.Mode != profileMode.Developer)
            return AppError.Forbidden("Switch to Developer profile to manage payout splits.");

        return null;
    }

    private async Task<bool> CanManageTeamAsync(Team team, CancellationToken ct)
    {
        if (team.OwnerUserId == _currentUser.UserId)
            return true;

        return await _teamMemberRepo.IsLeaderAsync(team.Id, _currentUser.UserId, ct);
    }

    private async Task<(AppError? Error, List<TeamPayoutSplit>? Splits)> BuildSplitsAsync(
        Guid teamId,
        Guid? projectId,
        Guid? milestoneId,
        Domain.Enums.SplitType splitType,
        IReadOnlyList<PayoutSplitItemDto> items,
        decimal totalAmount,
        CancellationToken ct,
        bool preferProjectMembers = false)
    {
        if (items is null || items.Count == 0)
            return (AppError.Validation("At least one split item is required."), null);

        var memberIds = (await _teamMemberRepo.GetByTeamIdAsync(teamId, ct))
            .Select(m => m.UserId)
            .ToHashSet();

        // Owner may not always be in TeamMembers — allow owner wallet share.
        var team = await _teamRepo.GetByIdAsync(teamId, ct);
        if (team is not null)
            memberIds.Add(team.OwnerUserId);

        if (preferProjectMembers && projectId is Guid pid)
        {
            var staffed = (await _projectMemberRepo.GetByProjectIdAsync(pid, ct))
                .Select(m => m.UserId)
                .ToHashSet();
            if (staffed.Count > 0)
                memberIds = staffed;
        }

        var entities = new List<TeamPayoutSplit>();
        foreach (var item in items)
        {
            if (item.UserId == Guid.Empty)
                return (AppError.Validation("Each split item needs a userId."), null);

            if (!memberIds.Contains(item.UserId))
                return (AppError.Validation($"User {item.UserId} is not eligible for this payout split."), null);

            if (item.Value <= 0)
                return (AppError.Validation("Each split value must be greater than 0."), null);

            entities.Add(new TeamPayoutSplit
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                ProjectId = projectId,
                MilestoneId = milestoneId,
                UserId = item.UserId,
                SplitType = splitType,
                Value = item.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId.ToString()
            });
        }

        if (!await _splitRepo.ValidateSplitsAsync(entities, totalAmount, ct))
        {
            return (AppError.Validation(splitType == Domain.Enums.SplitType.Percent
                ? "Percent splits must sum to 100."
                : $"Fixed splits must sum to {totalAmount:0.##}."), null);
        }

        return (null, entities);
    }

    private static bool TryParseSplitType(string? raw, out Domain.Enums.SplitType splitType)
    {
        splitType = Domain.Enums.SplitType.Percent;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        return Enum.TryParse(raw.Trim(), ignoreCase: true, out splitType);
    }

    private static PayoutSplitsDto ToDto(
        Guid teamId,
        Guid? projectId,
        Guid? milestoneId,
        IReadOnlyList<TeamPayoutSplit> splits)
    {
        var splitType = splits.Count > 0
            ? splits[0].SplitType.ToString()
            : nameof(Domain.Enums.SplitType.Percent);

        return new PayoutSplitsDto
        {
            TeamId = teamId,
            ProjectId = projectId,
            MilestoneId = milestoneId,
            SplitType = splitType,
            Items = splits.Select(s => new PayoutSplitItemDto
            {
                UserId = s.UserId,
                Value = s.Value
            }).ToList()
        };
    }
}
