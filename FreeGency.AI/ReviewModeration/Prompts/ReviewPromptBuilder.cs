using System.Text;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Models;

namespace FreeGency.AI.ReviewModeration.Prompts;

/// <summary>
/// Builds the per-request user prompt from the versioned system instruction and
/// the request context. Stateless and thread-safe.
/// </summary>
public sealed class ReviewPromptBuilder : IReviewPromptBuilder
{
    /// <summary>The maximum number of previous reviews surfaced to the model.</summary>
    private const int MaxPreviousReviews = 5;

    /// <inheritdoc />
    public ReviewPrompt Build(ReviewModerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userPrompt = BuildUserPrompt(request);
        return new ReviewPrompt(
            ReviewModerationSystemPrompt.System,
            userPrompt,
            ReviewModerationSystemPrompt.CurrentVersion);
    }

    private static string BuildUserPrompt(ReviewModerationRequest request)
    {
        var builder = new StringBuilder();

        builder.AppendLine("=== REVIEW CONTEXT ===");
        builder.AppendLine($"ReviewId: {request.ReviewId?.ToString() ?? "n/a"}");
        builder.AppendLine($"ProjectId: {request.ProjectId?.ToString() ?? "n/a"}");
        builder.AppendLine($"ReviewerId: {request.ReviewerId?.ToString() ?? "n/a"}");
        builder.AppendLine($"ReviewedUserId: {request.ReviewedUserId?.ToString() ?? "n/a"}");

        if (request.Rating is int rating)
            builder.AppendLine($"Rating: {rating}");

        if (!string.IsNullOrWhiteSpace(request.Language))
            builder.AppendLine($"Language: {request.Language}");

        if (request.CreatedAt != default)
            builder.AppendLine($"ReviewCreatedAt: {request.CreatedAt:O}");

        if (request.Metadata is { Count: > 0 })
        {
            builder.AppendLine();
            builder.AppendLine("=== ADDITIONAL CONTEXT ===");
            foreach (var (key, value) in request.Metadata)
                builder.AppendLine($"{key}: {value}");
        }

        if (request.PreviousReviews is { Count: > 0 } previous)
        {
            builder.AppendLine();
            builder.AppendLine("=== PREVIOUS REVIEWS BY THE SAME REVIEWER ===");
            var take = Math.Min(previous.Count, MaxPreviousReviews);
            for (var i = 0; i < take; i++)
            {
                var review = previous[i];
                builder.AppendLine($"- Previous review {i + 1}:");
                if (review.Rating is int previousRating)
                    builder.AppendLine($"  Rating: {previousRating}");
                if (!string.IsNullOrWhiteSpace(review.ReviewText))
                    builder.AppendLine($"  Text: {review.ReviewText}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("=== REVIEW TEXT TO MODERATE ===");
        builder.Append(request.ReviewText);

        return builder.ToString();
    }
}
