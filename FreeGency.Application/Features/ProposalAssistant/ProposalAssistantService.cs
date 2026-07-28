using System.Text;
using FreeGency.AI.ProposalAssistant;
using FreeGency.Application.Features.ProposalAssistant.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Application.Features.ProposalAssistant;

public sealed class ProposalAssistantService : IProposalAssistantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProposalAssistantChatService _chat;
    private readonly ICurrentUserService _currentUser;

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;

    public ProposalAssistantService(
        IUnitOfWork unitOfWork,
        ProposalAssistantChatService chat,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _chat = chat;
        _currentUser = currentUser;
        _projectRepository = _unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = _unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
    }

    public async Task<ApiResponse<ProposalAssistantResponseDto>> AskAsync(
        Guid projectId,
        ProposalAssistantRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return ApiResponse.Failure<ProposalAssistantResponseDto>(
                AppError.Validation("Message is required."));

        var project = await _projectRepository.Query()
            .Where(p => p.Id == projectId)
            .Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return ApiResponse.Failure<ProposalAssistantResponseDto>(
                AppError.NotFound(nameof(Project), projectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<ProposalAssistantResponseDto>(AppError.Forbidden());

        var proposals = await _proposalRepository.Query()
            .Where(p => p.ProjectId == projectId && p.Status != ProposalStatus.Withdrawn)
            .Include(p => p.Team).ThenInclude(t => t!.TeamSkills).ThenInclude(ts => ts.Skill)
            .Include(p => p.User).ThenInclude(u => u!.DeveloperProfile).ThenInclude(dp => dp!.UserSkills).ThenInclude(us => us.Skill)
            .OrderByDescending(p => p.AppliedAt)
            .Take(30)
            .ToListAsync(ct);

        var context = BuildContext(project, proposals);
        var history = (request.History ?? [])
            .Where(h => !string.IsNullOrWhiteSpace(h.Content))
            .TakeLast(6)
            .Select(h => (
                Role: string.Equals(h.Role, "assistant", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user",
                Content: h.Content.Trim()))
            .ToList();

        var command = NormalizeCommand(request.Command, request.Message);

        try
        {
            var ai = await _chat.AskAsync(
                context,
                request.Message.Trim(),
                command,
                request.FocusedApplicantName,
                history,
                ct);

            return ApiResponse.Success(MapResponse(ai, proposals));
        }
        catch (Exception ex)
        {
            return ApiResponse.Failure<ProposalAssistantResponseDto>(AppError.Failure(ex));
        }
    }

    private static string? NormalizeCommand(string? command, string message)
    {
        var raw = command;
        if (string.IsNullOrWhiteSpace(raw))
        {
            var trimmed = message.Trim();
            if (!trimmed.StartsWith('/'))
                return null;

            raw = trimmed[1..].Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var key = raw.Trim().TrimStart('/').ToLowerInvariant();
        return key switch
        {
            "summary" or "essentials" or "overview" => "summarize",
            "vs" or "versus" => "compare",
            "best" or "pick" or "winner" or "recommend" => "bestfit",
            "shortlist" or "top" or "order" => "rank",
            "flags" or "risks" or "risk" => "redflags",
            "who" or "applicant" => "profile",
            "msg" or "message" or "write" => "draft",
            "screen" or "interview" or "askthem" => "questions",
            "reason" or "explain" => "why",
            "cmds" or "commands" => "help",
            "reset" => "clear",
            _ => key
        };
    }

    private static string BuildContext(Project project, List<ProjectProposal> proposals)
    {
        var requiredSkills = project.ProjectSkills?
            .Select(ps => ps.Skill.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        var sb = new StringBuilder();
        sb.AppendLine($"ProjectId: {project.Id}");
        sb.AppendLine($"Title: {project.Title}");
        sb.AppendLine($"Description: {Truncate(project.Description, 1200)}");
        sb.AppendLine($"BudgetMin: {project.BudgetMin}");
        sb.AppendLine($"BudgetMax: {project.BudgetMax}");
        sb.AppendLine($"Currency: {project.Currency}");
        sb.AppendLine($"Deadline: {project.Deadline?.ToString("u") ?? "n/a"}");
        sb.AppendLine($"EstimatedDurationDays: {project.EstimatedDurationDays?.ToString() ?? "n/a"}");
        sb.AppendLine($"RequiredSkills: [{string.Join(", ", requiredSkills)}]");
        sb.AppendLine($"ProposalCount: {proposals.Count}");
        sb.AppendLine("Proposals:");

        foreach (var p in proposals)
        {
            var info = DescribeApplicant(p);
            var overlap = SkillOverlap(requiredSkills, info.Skills);
            var letterLen = p.CoverLetter?.Trim().Length ?? 0;
            var overBudget = p.ProposedBudget > project.BudgetMax;
            var underMin = p.ProposedBudget < project.BudgetMin;
            var delta = overBudget
                ? p.ProposedBudget - project.BudgetMax
                : underMin
                    ? p.ProposedBudget - project.BudgetMin
                    : 0;
            var flags = new List<string>();
            if (overBudget) flags.Add($"OverBudgetBy:{delta}");
            if (underMin) flags.Add($"UnderMinBy:{-delta}");
            if (letterLen < 80) flags.Add("ThinCoverLetter");
            if (letterLen >= 80 && LooksGeneric(p.CoverLetter)) flags.Add("GenericCoverLetter");
            if ((info.Reviews ?? 0) == 0) flags.Add("NoReviews");
            if (overlap < 0.34 && requiredSkills.Count > 0) flags.Add("LowSkillOverlap");

            sb.AppendLine("---");
            sb.AppendLine($"ProposalId: {p.Id}");
            sb.AppendLine($"ApplicantName: {info.Name}");
            sb.AppendLine($"ApplicantType: {info.Type}");
            sb.AppendLine($"UserId: {info.UserId ?? "n/a"}");
            sb.AppendLine($"TeamId: {info.TeamId ?? "n/a"}");
            sb.AppendLine($"Status: {p.Status}");
            sb.AppendLine($"ProposedBudget: {p.ProposedBudget}");
            sb.AppendLine($"BudgetDelta: {delta}");
            sb.AppendLine($"OverBudget: {(overBudget ? "yes" : "no")}");
            sb.AppendLine($"SkillOverlapPct: {Math.Round(overlap * 100)}");
            sb.AppendLine($"MatchedSkills: [{string.Join(", ", MatchedSkills(requiredSkills, info.Skills))}]");
            sb.AppendLine($"MissingSkills: [{string.Join(", ", MissingSkills(requiredSkills, info.Skills))}]");
            sb.AppendLine($"CoverLetterChars: {letterLen}");
            sb.AppendLine($"AverageRating: {info.Rating?.ToString("0.0") ?? "n/a"}");
            sb.AppendLine($"ReviewCount: {info.Reviews?.ToString() ?? "n/a"}");
            sb.AppendLine($"Skills: [{string.Join(", ", info.Skills)}]");
            sb.AppendLine($"ComputedFlags: [{string.Join(", ", flags)}]");
            sb.AppendLine($"CoverLetter: {Truncate(p.CoverLetter, 1400)}");
            sb.AppendLine($"AvatarUrl: {info.Avatar ?? "n/a"}");
        }

        return sb.ToString();
    }

    private static double SkillOverlap(List<string> required, List<string> applicant)
    {
        if (required.Count == 0) return 0.5;
        if (applicant.Count == 0) return 0;
        var set = applicant.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hits = required.Count(r => set.Contains(r));
        return (double)hits / required.Count;
    }

    private static IEnumerable<string> MatchedSkills(List<string> required, List<string> applicant)
    {
        var set = applicant.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return required.Where(r => set.Contains(r));
    }

    private static IEnumerable<string> MissingSkills(List<string> required, List<string> applicant)
    {
        var set = applicant.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return required.Where(r => !set.Contains(r));
    }

    private static bool LooksGeneric(string? cover)
    {
        if (string.IsNullOrWhiteSpace(cover) || cover.Length < 80) return false;
        var lower = cover.ToLowerInvariant();
        string[] markers =
        [
            "i am writing to express",
            "dear hiring",
            "with all due respect",
            "i am a passionate",
            "looking forward to hearing from you"
        ];
        return markers.Count(m => lower.Contains(m)) >= 2;
    }

    private static ApplicantInfo DescribeApplicant(ProjectProposal proposal)
    {
        if (proposal.ApplicantType == ApplicantType.Team && proposal.Team is not null)
        {
            var teamSkills = proposal.Team.TeamSkills?
                .Select(ts => ts.Skill.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList() ?? [];

            return new ApplicantInfo(
                proposal.Team.Name,
                "Team",
                (double)proposal.Team.AverageRating,
                proposal.Team.RatingCount,
                teamSkills,
                proposal.Team.Logo,
                null,
                proposal.TeamId?.ToString());
        }

        if (proposal.User is not null)
        {
            var profile = proposal.User.DeveloperProfile;
            var userSkills = profile?.UserSkills?
                .Select(us => us.Skill.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList() ?? [];

            return new ApplicantInfo(
                $"{proposal.User.FristName} {proposal.User.LastName}".Trim(),
                "User",
                profile is not null ? (double)profile.AverageRating : null,
                profile?.RatingCount,
                userSkills,
                profile?.ProfileImage,
                proposal.UserId?.ToString(),
                null);
        }

        return new ApplicantInfo(
            "Unknown Applicant",
            proposal.ApplicantType.ToString(),
            null,
            null,
            [],
            null,
            proposal.UserId?.ToString(),
            proposal.TeamId?.ToString());
    }

    private static ProposalAssistantResponseDto MapResponse(
        ProposalAssistantAiResult ai,
        List<ProjectProposal> proposals)
    {
        var cards = (ai.Cards ?? []).Select(card =>
        {
            var match = FindProposal(proposals, card);
            var info = match is not null
                ? DescribeApplicant(match)
                : new ApplicantInfo(card.ApplicantName, "User", null, null, [], null, null, null);

            return new ProposalAssistantCardDto
            {
                Type = "profile",
                ApplicantName = string.IsNullOrWhiteSpace(card.ApplicantName) ? info.Name : card.ApplicantName,
                ProposalId = card.ProposalId ?? match?.Id.ToString(),
                UserId = card.UserId ?? info.UserId ?? match?.UserId?.ToString(),
                TeamId = card.TeamId ?? info.TeamId ?? match?.TeamId?.ToString(),
                AvatarUrl = info.Avatar,
                ApplicantType = match is not null ? info.Type : null,
                Rating = card.Rating ?? info.Rating,
                ReviewCount = card.ReviewCount ?? info.Reviews,
                Skills = (card.Skills is { Count: > 0 } ? card.Skills : info.Skills) ?? [],
                Highlights = card.Highlights ?? [],
                ProposedBudget = card.ProposedBudget ?? match?.ProposedBudget,
                CoverSnippet = match?.CoverLetter is { Length: > 0 } c ? Truncate(c, 280) : null,
                Insight = string.IsNullOrWhiteSpace(card.Insight) ? null : card.Insight.Trim()
            };
        }).ToList();

        return new ProposalAssistantResponseDto
        {
            Reply = ai.Reply,
            Intent = string.IsNullOrWhiteSpace(ai.Intent) ? "ask" : ai.Intent.ToLowerInvariant(),
            Cards = cards,
            Chips = string.Equals(ai.Intent, "clarify", StringComparison.OrdinalIgnoreCase)
                && ai.Chips is { Count: > 0 }
                ? ai.Chips.Take(5).ToList()
                : [],
            Actions = (ai.Actions ?? []).Select(a => new ProposalAssistantActionDto
            {
                Type = a.Type,
                UserId = a.UserId,
                TeamId = a.TeamId,
                ProposalId = a.ProposalId,
                ProjectId = a.ProjectId,
                Label = a.Label
            }).ToList()
        };
    }

    private static ProjectProposal? FindProposal(List<ProjectProposal> proposals, ProposalAssistantAiCard card)
    {
        if (Guid.TryParse(card.ProposalId, out var id))
        {
            var byId = proposals.FirstOrDefault(p => p.Id == id);
            if (byId is not null) return byId;
        }

        if (string.IsNullOrWhiteSpace(card.ApplicantName))
            return null;

        return proposals.FirstOrDefault(p =>
        {
            var name = DescribeApplicant(p).Name;
            return name.Equals(card.ApplicantName, StringComparison.OrdinalIgnoreCase)
                   || name.Contains(card.ApplicantName, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max] + "…";
    }

    private sealed record ApplicantInfo(
        string Name,
        string Type,
        double? Rating,
        int? Reviews,
        List<string> Skills,
        string? Avatar,
        string? UserId,
        string? TeamId);
}
