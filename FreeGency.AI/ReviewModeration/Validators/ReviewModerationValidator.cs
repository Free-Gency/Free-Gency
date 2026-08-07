using System.Text;
using FreeGency.AI.ReviewModeration.DTOs;

namespace FreeGency.AI.ReviewModeration.Validators;

/// <summary>
/// Validates a <see cref="ReviewModerationRequest"/> and normalizes its text.
/// Stateless and thread-safe.
/// </summary>
public static class ReviewModerationValidator
{
    /// <summary>The maximum accepted review length in characters.</summary>
    public const int MaxReviewLength = 10000;

    /// <summary>
    /// Validates the request: rejects null requests, empty reviews, reviews
    /// longer than <see cref="MaxReviewLength"/>, and ratings outside 1..5.
    /// </summary>
    public static bool IsValid(ReviewModerationRequest request, out string? error)
    {
        error = null;

        if (request is null)
        {
            error = "The review request is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.ReviewText))
        {
            error = "Review text is required.";
            return false;
        }

        if (request.ReviewText.Length > MaxReviewLength)
        {
            error = $"Review text exceeds the maximum of {MaxReviewLength} characters.";
            return false;
        }

        if (request.Rating is int rating && (rating < 1 || rating > 5))
        {
            error = "Rating must be between 1 and 5.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizes review text: Unicode normalization form KC followed by trimming.
    /// Empty or whitespace-only input returns an empty string.
    /// </summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text.Normalize(NormalizationForm.FormKC).Trim();
    }
}
