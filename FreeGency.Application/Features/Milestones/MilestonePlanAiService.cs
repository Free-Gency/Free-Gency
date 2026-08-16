using System.Globalization;
using System.Text;
using FreeGency.AI.MilestonePlanAssist;
using FreeGency.Application.Features.Milestones.DTOs;
using FreeGency.Domain.Interfaces.Repositories.Teams;

namespace FreeGency.Application.Features.Milestones;

public sealed class MilestonePlanAiService : IMilestonePlanAiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IEntitlementService _entitlementService;
    private readonly MilestonePlanAssistChatService _chat;

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IMilestonePlanVersionRepository _planRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;

    public MilestonePlanAiService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IEntitlementService entitlementService,
        MilestonePlanAssistChatService chat)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _entitlementService = entitlementService;
        _chat = chat;

        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _planRepository = unitOfWork.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _teamMemberRepository = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
    }

    public async Task<ApiResponse<MilestonePlanAiAssistResponseDto>> AssistAsync(
        Guid projectId,
        MilestonePlanAiAssistRequestDto request,
        CancellationToken ct = default)
    {
        if (request.ProposalId == Guid.Empty)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                AppError.Validation("ProposalId is required."));

        var profileError = await RequireDeveloperModeAsync(ct);
        if (profileError is not null)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(profileError);

        var project = await _projectRepository.Query()
            .Where(p => p.Id == projectId)
            .Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .Include(p => p.ProjectSpecialties).ThenInclude(ps => ps.Specialty)
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                AppError.NotFound(nameof(Project), projectId));

        var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                AppError.NotFound(nameof(ProjectProposal), request.ProposalId));

        if (proposal.ProjectId != projectId)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                AppError.Validation("Proposal does not belong to this project."));

        if (proposal.Status != ProposalStatus.InDiscussion)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                AppError.Validation("Proposal must be In Discussion to use plan assist."));

        if (!await IsProposalNegotiationSpeakerAsync(proposal, ct))
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                AppError.Forbidden("Only the negotiation speaker can use plan assist."));

        var mode = MapMode(request.Mode);
        var current = (request.CurrentMilestones ?? [])
            .Select(MapDraft)
            .ToList();

        if (mode is MilestonePlanAssistMode.Milestone or MilestonePlanAssistMode.Field)
        {
            if (request.MilestoneIndex is null || request.MilestoneIndex < 0)
                return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                    AppError.Validation("MilestoneIndex is required for this mode."));

            if (request.MilestoneIndex >= Math.Max(current.Count, 1) && current.Count > 0)
                return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                    AppError.Validation("MilestoneIndex is out of range."));

            if (current.Count == 0)
            {
                current.Add(new MilestonePlanAssistDraftItem());
            }
            else if (request.MilestoneIndex >= current.Count)
            {
                return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                    AppError.Validation("MilestoneIndex is out of range."));
            }
        }

        string? field = null;
        if (mode == MilestonePlanAssistMode.Field)
        {
            field = NormalizeField(request.Field);
            if (field is null)
                return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                    AppError.Validation("Field must be title or definitionOfDone."));
        }

        string? changeComment = request.ChangeComment?.Trim();
        if (mode == MilestonePlanAssistMode.ApplyChangeRequest)
        {
            if (string.IsNullOrWhiteSpace(changeComment))
            {
                var latest = await _planRepository.GetLatestByProposalIdAsync(request.ProposalId, ct);
                changeComment = latest?.ChangeComment?.Trim();
            }

            if (string.IsNullOrWhiteSpace(changeComment))
                return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(
                    AppError.Validation("A change-request comment is required to apply hiring agent edits."));

            if (current.Count == 0)
            {
                var latest = await _planRepository.GetLatestByProposalIdAsync(request.ProposalId, ct);
                if (latest?.Items is { Count: > 0 })
                {
                    current = latest.Items
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new MilestonePlanAssistDraftItem
                        {
                            Title = i.Title,
                            DefinitionOfDone = i.DefinitionOfDone,
                            Amount = i.Amount,
                            DueDate = i.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        })
                        .ToList();
                }
            }
        }

        var quota = await _entitlementService.CanConsumeAsync(_currentUser.UserId, FeatureType.AIChatProposal, ct);
        if (!quota.IsAllowed)
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(quota.ToAppError());

        try
        {
            var result = await _chat.AssistAsync(
                BuildProjectContext(project, request.ProposalId, _currentUser.UserId),
                mode,
                current,
                request.MilestoneIndex,
                field,
                changeComment,
                ct);

            await _entitlementService.ConsumeAsync(_currentUser.UserId, FeatureType.AIChatProposal, ct);

            return ApiResponse.Success(new MilestonePlanAiAssistResponseDto
            {
                Milestones = result.Milestones.Select(m => new MilestonePlanAiDraftItemDto
                {
                    Title = m.Title?.Trim() ?? string.Empty,
                    DefinitionOfDone = m.DefinitionOfDone?.Trim() ?? string.Empty,
                    Amount = m.Amount < 0 ? 0 : m.Amount,
                    DueDate = m.DueDate
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.Failure<MilestonePlanAiAssistResponseDto>(AppError.Failure(ex));
        }
    }

    private static MilestonePlanAssistMode MapMode(MilestonePlanAiAssistMode mode) => mode switch
    {
        MilestonePlanAiAssistMode.ApplyChangeRequest => MilestonePlanAssistMode.ApplyChangeRequest,
        MilestonePlanAiAssistMode.Milestone => MilestonePlanAssistMode.Milestone,
        MilestonePlanAiAssistMode.Field => MilestonePlanAssistMode.Field,
        _ => MilestonePlanAssistMode.FullPlan
    };

    private static MilestonePlanAssistDraftItem MapDraft(MilestonePlanAiDraftItemDto dto) => new()
    {
        Title = dto.Title?.Trim() ?? string.Empty,
        DefinitionOfDone = dto.DefinitionOfDone?.Trim() ?? string.Empty,
        Amount = dto.Amount,
        DueDate = string.IsNullOrWhiteSpace(dto.DueDate) ? null : dto.DueDate.Trim()
    };

    private static string? NormalizeField(string? field)
    {
        if (string.IsNullOrWhiteSpace(field)) return null;
        var key = field.Trim().ToLowerInvariant();
        return key switch
        {
            "title" => "title",
            "definitionofdone" or "definition_of_done" or "dod" => "definitionOfDone",
            _ => null
        };
    }

    private static string BuildProjectContext(Project project, Guid proposalId, Guid userId)
    {
        var skills = project.ProjectSkills?
            .Select(ps => ps.Skill?.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        var specialties = project.ProjectSpecialties?
            .Select(ps => ps.Specialty?.NameEn)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        var sb = new StringBuilder();
        sb.AppendLine($"Title: {project.Title}");
        sb.AppendLine($"Description: {Truncate(project.Description, 1600)}");
        sb.AppendLine($"IsFixedPrice: {project.IsFixedPrice}");
        sb.AppendLine($"BudgetMin: {project.BudgetMin}");
        sb.AppendLine($"BudgetMax: {project.BudgetMax}");
        sb.AppendLine($"Currency: {project.Currency}");
        sb.AppendLine($"Deadline: {project.Deadline?.ToString("yyyy-MM-dd") ?? "n/a"}");
        sb.AppendLine($"EstimatedDurationDays: {project.EstimatedDurationDays?.ToString() ?? "n/a"}");
        sb.AppendLine($"Skills: [{string.Join(", ", skills)}]");
        sb.AppendLine($"Specialties: [{string.Join(", ", specialties)}]");
        sb.AppendLine($"TodayUtc: {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine($"ProposalId: {proposalId}");
        sb.AppendLine($"RequesterUserId: {userId}");
        sb.AppendLine($"GenerationNonce: {Guid.NewGuid():N}");
        return sb.ToString();
    }

    private async Task<AppError?> RequireDeveloperModeAsync(CancellationToken ct)
    {
        var active = await _userRepository.GetActiveProfileAsync(_currentUser.UserId, ct);
        if (active is null)
            return AppError.Validation(
                "An active profile is required. Create or switch to a Client or Developer profile.");

        if (active.Value.Mode != profileMode.Developer)
            return AppError.Forbidden("Switch to Developer profile to perform this action.");

        return null;
    }

    private async Task<bool> IsProposalNegotiationSpeakerAsync(ProjectProposal proposal, CancellationToken ct)
    {
        if (proposal.UserId != _currentUser.UserId)
            return false;

        if (proposal.ApplicantType == ApplicantType.User)
            return true;

        return proposal.TeamId is not null &&
               await _teamMemberRepository.IsLeaderAsync(proposal.TeamId.Value, _currentUser.UserId, ct);
    }

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max] + "…";
    }
}
