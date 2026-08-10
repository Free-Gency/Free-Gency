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
    private readonly IUserRepository _userRepo;

    public PayoutSplitService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _teamRepo = _unitOfWork.Repository<ITeamRepository, Team>();
        _teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _splitRepo = _unitOfWork.Repository<ITeamPayoutSplitRepository, TeamPayoutSplit>();
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
        _userRepo = _unitOfWork.Repository<IUserRepository, User>();
    }

    public async Task<ApiResponse<PayoutSplitsDto>> GetTeamDefaultsAsync(Guid teamId, CancellationToken ct = default)
    {
        var team = await _teamRepo.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.NotFound(nameof(Team), teamId));

        if (!await CanManageTeamAsync(team, ct))
            return ApiResponse.Failure<PayoutSplitsDto>(AppError.Forbidden("Only the team owner or leader can view payout splits."));

        var splits = (await _splitRepo.GetByTeamAndProjectAsync(teamId, null, ct)).ToList();
        return ApiResponse.Success(ToDto(teamId, null, splits));
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

        var buildError = await BuildSplitsAsync(teamId, null, splitType, dto.Items, totalAmount: 100m, ct);
        if (buildError.Error is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(buildError.Error);

        await _splitRepo.ReplaceSplitsAsync(teamId, null, buildError.Splits!, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = (await _splitRepo.GetByTeamAndProjectAsync(teamId, null, ct)).ToList();
        return ApiResponse.Success(ToDto(teamId, null, saved), "Team payout splits updated.");
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

        var projectSplits = (await _splitRepo.GetByTeamAndProjectAsync(team.Id, projectId, ct)).ToList();
        if (projectSplits.Count > 0)
            return ApiResponse.Success(ToDto(team.Id, projectId, projectSplits));

        // Fallback: team defaults
        var defaults = (await _splitRepo.GetByTeamAndProjectAsync(team.Id, null, ct)).ToList();
        return ApiResponse.Success(ToDto(team.Id, projectId: null, defaults));
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
            splitType,
            dto.Items,
            totalForValidation,
            ct);
        if (buildError.Error is not null)
            return ApiResponse.Failure<PayoutSplitsDto>(buildError.Error);

        await _splitRepo.ReplaceSplitsAsync(team.Id, projectId, buildError.Splits!, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = (await _splitRepo.GetByTeamAndProjectAsync(team.Id, projectId, ct)).ToList();
        return ApiResponse.Success(ToDto(team.Id, projectId, saved), "Project payout splits updated.");
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
        Domain.Enums.SplitType splitType,
        IReadOnlyList<PayoutSplitItemDto> items,
        decimal totalAmount,
        CancellationToken ct)
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

        var entities = new List<TeamPayoutSplit>();
        foreach (var item in items)
        {
            if (item.UserId == Guid.Empty)
                return (AppError.Validation("Each split item needs a userId."), null);

            if (!memberIds.Contains(item.UserId))
                return (AppError.Validation($"User {item.UserId} is not a member of this team."), null);

            if (item.Value <= 0)
                return (AppError.Validation("Each split value must be greater than 0."), null);

            entities.Add(new TeamPayoutSplit
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                ProjectId = projectId,
                UserId = item.UserId,
                SplitType = splitType,
                Value = item.Value
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

    private static PayoutSplitsDto ToDto(Guid teamId, Guid? projectId, IReadOnlyList<TeamPayoutSplit> splits)
    {
        var splitType = splits.Count > 0
            ? splits[0].SplitType.ToString()
            : nameof(Domain.Enums.SplitType.Percent);

        return new PayoutSplitsDto
        {
            TeamId = teamId,
            ProjectId = projectId,
            SplitType = splitType,
            Items = splits.Select(s => new PayoutSplitItemDto
            {
                UserId = s.UserId,
                Value = s.Value
            }).ToList()
        };
    }
}
