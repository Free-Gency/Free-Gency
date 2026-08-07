
using System.Text.Json.Serialization;

namespace FreeGency.AI.DTOs;


// --- Request (Application maps domain → this) ---
public sealed class TeamSuggestionRequest
{
    public string DeveloperName { get; set; } = string.Empty;
    public List<string> DeveloperSkills { get; set; } = [];
    public List<string> DeveloperSpecialties { get; set; } = [];
    public List<string> DeveloperCategories { get; set; } = [];
    public List<TeamCandidateInput> Candidates { get; set; } = [];
    public int TopK { get; set; } = 10;
}

public sealed class TeamCandidateInput
{
    public string TeamId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public List<string> Specialties { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    public int MemberCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<OpenJobInput> OpenJobs { get; set; } = [];
}

public sealed class OpenJobInput
{
    public string JobId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
}

// --- Result (parsed LLM output) ---
public sealed class TeamSuggestionAiResult
{
    [JsonPropertyName("candidates")]
    public List<TeamSuggestionAiCandidate> Candidates { get; set; } = [];

    [JsonPropertyName("overallSummary")]
    public string OverallSummary { get; set; } = string.Empty;
}

public sealed class TeamSuggestionAiCandidate
{
    [JsonPropertyName("teamId")]
    public string TeamId { get; set; } = string.Empty;

    [JsonPropertyName("jobId")]
    public string? JobId { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = [];

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = [];
}
