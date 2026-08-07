using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Contracts;

/// <summary>
/// Builds a fully versioned review moderation prompt from a request. Implementations
/// load the template, inject the request variables, version the prompt, and return
/// the final <see cref="ReviewPrompt"/>. They never call the AI layer.
/// </summary>
public interface IReviewPromptBuilder
{
    /// <summary>
    /// Builds the review moderation prompt for the given request.
    /// </summary>
    ReviewPrompt Build(ReviewModerationRequest request);
}
