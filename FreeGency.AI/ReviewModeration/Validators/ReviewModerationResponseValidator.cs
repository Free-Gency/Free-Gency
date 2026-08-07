using System.Text.RegularExpressions;
using FreeGency.AI.ReviewModeration.Providers;

namespace FreeGency.AI.ReviewModeration.Validators;

/// <summary>
/// Validates the AI moderation output before it is trusted: every required
/// property must be present, and the raw output must not leak secrets, prompt
/// templates, hidden instructions, internal architecture, stack traces, database
/// or configuration details, API keys, or internal logs. Deterministic and
/// synchronous. Failing validation is a transient condition: the reliability
/// layer retries once and, if it still fails, the service falls back to manual
/// review.
/// </summary>
public sealed class ReviewModerationResponseValidator
{
    private static readonly LeakPattern[] LeakPatterns = BuildLeakPatterns();

    private static readonly (string Field, Func<ReviewAiResponse, bool> Check)[] Required =
    {
        ("RiskScore", ai => ai.RiskScore is not null),
        ("Confidence", ai => ai.Confidence is not null),
        ("Action", ai => !string.IsNullOrWhiteSpace(ai.Action)),
        ("Categories", ai => ai.Categories.Count > 0),
        ("Summary", ai => ai.Summary is not null || !string.IsNullOrWhiteSpace(ai.FlatSummary)),
        ("QualityScore", ai => ai.QualityScore is not null),
        ("Sentiment", ai => !string.IsNullOrWhiteSpace(ai.Sentiment))
    };

    /// <summary>
    /// Validates the raw model output. Returns a result describing only stable,
    /// content-free metadata: which fields were missing and which leak signals
    /// fired. Raw text is never included.
    /// </summary>
    public ReviewResponseValidationResult Validate(string? raw, ReviewAiResponse? ai)
    {
        var missing = new List<string>();
        var leaks = new List<string>();

        if (ai is not null)
        {
            foreach (var (field, check) in Required)
            {
                if (!check(ai))
                    missing.Add(field);
            }
        }

        if (!string.IsNullOrWhiteSpace(raw))
        {
            foreach (var pattern in LeakPatterns)
            {
                if (pattern.Regex.IsMatch(raw))
                    leaks.Add(pattern.Signal);
            }
        }

        var incomplete = missing.Count > 0;
        var leaking = leaks.Count > 0;

        string? reason = null;
        if (incomplete && leaking)
            reason = "The AI response is missing required fields and leaked protected content.";
        else if (incomplete)
            reason = "The AI response is missing required fields.";
        else if (leaking)
            reason = "The AI response leaked protected content.";

        return new ReviewResponseValidationResult(!incomplete && !leaking, incomplete, leaking, missing, leaks, reason);
    }

    private static LeakPattern[] BuildLeakPatterns()
    {
        var raw = new (string RegexPattern, string Signal)[]
        {
            // API keys / tokens / secrets
            (@"\b(?:sk-[A-Za-z0-9]{16,}|pk-[A-Za-z0-9]{16,}|AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{20,}|xox[bap]-[A-Za-z0-9\-]{10,}|AIza[0-9A-Za-z\-]{20,})\b", "api-key"),
            (@"-----BEGIN [A-Z0-9 ]*PRIVATE KEY-----", "private-key"),
            (@"\beyJ[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}\b", "jwt"),
            (@"\b(?:api\s*key|secret|password)\s*[=:]\s*\S+", "secret"),
            // Connection strings / database
            (@"connection\s*string", "connection-string"),
            (@"\bServer\s*=\s*[^;]+;", "connection-string"),
            (@"\bData\s+Source\s*=", "connection-string"),
            (@"\bIntegrated\s+Security\s*=", "connection-string"),
            (@"\b(?:AddDbContext|UseSqlServer|UseNpgsql|UseMySql)\b", "database-config"),
            // Configuration
            (@"\bappsettings\b", "config"),
            (@"\bconnectionStrings\b", "config"),
            (@"\benvironment\s+variables\b", "config"),
            // Stack traces
            (@"\bat\s+(?:FreeGency|System|Microsoft)\.[A-Za-z0-9_.]+\(.*?\)", "stack-trace"),
            (@"\.cs\s*:\s*line\s+\d+", "stack-trace"),
            (@"\bstack\s+trace\b", "stack-trace"),
            (@"\bunhandled\s+exception\b", "stack-trace"),
            // Prompt templates / hidden instructions
            (@"\bsystem\s+prompt\b", "prompt-template"),
            (@"\bprompt\s+template\b", "prompt-template"),
            (@"\binstruction\s+template\b", "prompt-template"),
            (@"\byou\s+are\s+a\s+helpful\s+assistant\b", "prompt-template"),
            (@"\bhidden\s+instructions\b", "prompt-template"),
            (@"\{\{[A-Za-z_.]+\}\}", "prompt-template"),
            // Internal architecture / code
            (@"\bnamespace\s+FreeGency\b", "architecture"),
            (@"\binternal\s+class\b", "architecture"),
            (@"\b(?:SemanticKernel|BedrockGateway|IChatCompletionService|IReviewModerationService)\b", "architecture"),
            (@"\bprompt\s+builder\b", "architecture"),
            // Internal logs
            (@"\b(?:ILogger|LogInformation|LogWarning|LogError|LogDebug)\b", "internal-logs"),
            (@"\bcorrelation\s+id\b", "internal-logs")
        };

        return raw
            .Select(r => new LeakPattern(new Regex(r.RegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase), r.Signal))
            .ToArray();
    }

    private sealed record LeakPattern(Regex Regex, string Signal);
}

/// <summary>
/// The outcome of <see cref="ReviewModerationResponseValidator.Validate"/>. Carries
/// only stable, content-free metadata: which required fields were missing and
/// which leak signals fired. The reason string is a fixed label, never raw text.
/// </summary>
/// <param name="IsValid">True when all required fields are present and no leak was detected.</param>
/// <param name="HasIncompleteFields">True when at least one required field was missing.</param>
/// <param name="HasLeak">True when a data-leakage signal fired.</param>
/// <param name="MissingFields">The names of the missing required fields.</param>
/// <param name="LeakSignals">The leak signal labels that fired.</param>
/// <param name="Reason">A fixed, content-free reason label for logging.</param>
public sealed record ReviewResponseValidationResult(
    bool IsValid,
    bool HasIncompleteFields,
    bool HasLeak,
    IReadOnlyList<string> MissingFields,
    IReadOnlyList<string> LeakSignals,
    string? Reason);
