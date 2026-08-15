using System.Text;

namespace FreeGency.AI.Common;

/// <summary>
/// Sanitizes untrusted user- or data-driven content before it is embedded in an agent
/// prompt, and wraps each payload section in explicit <c>&lt;&lt;&lt;label&gt;&gt;&gt;</c>
/// markers. The markers give the model a clear "this is data, not instructions" boundary
/// (mirrored by the security rules in the system prompts) and keep prompt-injection
/// attempts visually separated from the real instructions.
/// </summary>
internal static class PromptInputSanitizer
{
    internal const int DefaultMaxLength = 6000;

    /// <summary>
    /// Normalizes line endings, strips control characters (except \n and \t), trims, and
    /// truncates to <paramref name="maxLength"/> characters at a word boundary.
    /// </summary>
    internal static string Sanitize(string? value, int maxLength = DefaultMaxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var text = value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);

        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch is '\n' or '\t' || !char.IsControl(ch))
                builder.Append(ch);
        }

        var cleaned = builder.ToString().Trim();
        if (cleaned.Length <= maxLength)
            return cleaned;

        var cut = cleaned[..maxLength];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > maxLength / 2)
            cut = cut[..lastSpace];

        return cut.TrimEnd(' ', ',', '.', ';', ':') + "…";
    }

    /// <summary>
    /// Wraps a value in the <c>&lt;&lt;&lt;label&gt;&gt;&gt;</c> ... <c>&lt;&lt;&lt;end label&gt;&gt;&gt;</c>
    /// data markers. Empty input becomes "(none)".
    /// </summary>
    internal static string Section(string label, string? content)
    {
        var sanitized = Sanitize(content);
        var body = sanitized.Length == 0 ? "(none)" : sanitized;
        return $"<<<{label}>>>\n{body}\n<<<end {label}>>>";
    }
}
