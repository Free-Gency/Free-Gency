using System.Security.Cryptography;
using System.Text;
using FreeGency.AI.ChatModeration.Constants;
using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.Security;

/// <summary>
/// Builds deterministic SHA-256 cache keys for moderated content. Thread-safe
/// and stateless.
///
/// The key combines the message, conversation type, language, project id and
/// prompt version. Because the prompt version is part of the key, bumping
/// <see cref="ChatSecurityCacheConstants.PromptVersion"/> invalidates every old
/// entry automatically.
/// </summary>
public sealed class ChatSecurityCacheKeyBuilder
{
    private const string HashSeparator = "\u001F";

    /// <summary>
    /// Builds a stable key for the given moderation inputs. The message is
    /// expected to be already normalized (see <see cref="MessageNormalizer"/>)
    /// so that spelling variants of the same message share one cache entry.
    /// </summary>
    public string BuildKey(
        string message,
        ConversationType conversationType,
        ContentLanguage language,
        string? projectId,
        string promptVersion)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(promptVersion);

        var canonical = string.Join(
            HashSeparator,
            message,
            conversationType.ToString(),
            language.ToString(),
            projectId ?? string.Empty,
            promptVersion);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return ChatSecurityCacheConstants.CacheKeyPrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
