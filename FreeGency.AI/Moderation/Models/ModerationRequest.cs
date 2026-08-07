namespace FreeGency.AI.Moderation.Models;

/// <summary>
/// The input to the moderation core. Feature-agnostic: <see cref="ContentKind"/>
/// describes what kind of user-generated content is being moderated (for example
/// <c>ChatMessage</c>, <c>Review</c>, <c>Comment</c>, <c>ProjectDescription</c>,
/// <c>Proposal</c>, <c>Profile</c> or <c>SupportTicket</c>).
/// </summary>
public sealed class ModerationRequest
{
    /// <summary>Gets or sets the content to moderate. Never logged or cached in raw form.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind of content being moderated. When null, the engine
    /// treats the content as generic user-generated content.
    /// </summary>
    public string? ContentKind { get; set; }

    /// <summary>
    /// Gets or sets surrounding context that helps interpretation (for example
    /// previous messages or the parent project description). Optional.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets optional key/value context (for example author role or a
    /// locale hint). Values are passed to the model prompt only.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
