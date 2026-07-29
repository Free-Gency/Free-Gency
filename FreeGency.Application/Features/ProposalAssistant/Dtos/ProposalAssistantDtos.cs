namespace FreeGency.Application.Features.ProposalAssistant.Dtos;

public sealed class ProposalAssistantRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? Command { get; set; }
    public List<ProposalAssistantHistoryItemDto>? History { get; set; }
    public string? FocusedApplicantName { get; set; }
}

public sealed class ProposalAssistantHistoryItemDto
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public sealed class ProposalAssistantResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public string Intent { get; set; } = "ask";
    public List<ProposalAssistantCardDto> Cards { get; set; } = [];
    public List<string> Chips { get; set; } = [];
    public List<ProposalAssistantActionDto> Actions { get; set; } = [];
}

public sealed class ProposalAssistantCardDto
{
    public string Type { get; set; } = "profile";
    public string ApplicantName { get; set; } = string.Empty;
    public string? ProposalId { get; set; }
    public string? UserId { get; set; }
    public string? TeamId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ApplicantType { get; set; }
    public double? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public List<string>? Skills { get; set; }
    public List<string>? Highlights { get; set; }
    public decimal? ProposedBudget { get; set; }
    public string? CoverSnippet { get; set; }
    public string? Insight { get; set; }
}

public sealed class ProposalAssistantActionDto
{
    public string Type { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? TeamId { get; set; }
    public string? ProposalId { get; set; }
    public string? ProjectId { get; set; }
    public string? Label { get; set; }
}
