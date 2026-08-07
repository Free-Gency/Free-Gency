using System.Text.RegularExpressions;
using FreeGency.AI.ReviewModeration.Contracts;
using FreeGency.AI.ReviewModeration.DTOs;
using FreeGency.AI.ReviewModeration.Validators;
using FreeGency.Api.Contracts;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Results;

namespace FreeGency.Api.Controllers.V1;

/// <summary>
/// AI Review Moderation API.
///
/// Exposes the <c>POST /api/v1/ai/reviews/moderate</c> endpoint that runs the
/// fully instrumented review moderation pipeline (security analysis,
/// deterministic intelligence, AI verdict, caching and observability) over a
/// marketplace review and returns a structured verdict.
///
/// The design is intentionally surface-agnostic: the same request/response
/// contract can later power comments, chat, proposals, project descriptions
/// and profiles without any architectural change.
/// </summary>
[Route("api/v1/ai/reviews")]
[Authorize]
public class AIReviewModerationController(
    IReviewModerationService reviewModerationService) : BaseApiController
{
    /// <summary>Moderates a marketplace review and returns whether it is safe to publish.</summary>
    /// <remarks>
    /// The review is validated before any work is done: empty reviews, reviews
    /// longer than <see cref="ReviewModerationValidator.MaxReviewLength"/>
    /// characters, ratings outside 1..5, requests with no review identifier,
    /// and malformed language hints are all rejected with a 400
    /// <c>Validation.Failed</c> error. Valid requests run the full moderation
    /// pipeline and never fail; when the AI service is unavailable the verdict
    /// falls back to a deterministic result.
    ///
    /// Example request:
    /// <code>
    /// {
    ///   "reviewId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "reviewerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "reviewedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "reviewText": "Excellent freelancer with outstanding communication. Delivered on time!",
    ///   "rating": 5,
    ///   "language": "en"
    /// }
    /// </code>
    ///
    /// Example response (200): enums serialize as their numeric values
    /// (<see cref="FreeGency.AI.ReviewModeration.Enums.RiskLevel"/> 0 = Safe,
    /// <see cref="FreeGency.AI.ReviewModeration.Enums.ReviewAction"/> 0 = Allow,
    /// <see cref="FreeGency.AI.ReviewModeration.Enums.ReviewSentiment"/> 1 = Positive).
    /// The frontend contract exposes <c>status</c> (display badge: Safe, Warning,
    /// Masked, Rejected, or Manual Review), <c>cached</c> and <c>model</c>.
    /// <code>
    /// {
    ///   "isSuccess": true,
    ///   "data": {
    ///     "approved": true,
    ///     "riskScore": 8,
    ///     "confidence": 0.97,
    ///     "riskLevel": 0,
    ///     "action": 0,
    ///     "status": "Safe",
    ///     "sentiment": 1,
    ///     "sentimentConfidence": 0.93,
    ///     "qualityScore": 88,
    ///     "qualityBand": 1,
    ///     "authenticityScore": 98,
    ///     "toxicityScore": 0,
    ///     "summary": {
    ///       "shortSummary": "Excellent freelancer with outstanding communication.",
    ///       "positivePoints": ["Reliable", "High Quality"],
    ///       "negativePoints": [],
    ///       "keyTopics": ["Communication"]
    ///     },
    ///     "reason": "The review is polite, specific, and consistent with the rating.",
    ///     "detectedCategories": [],
    ///     "securityCategories": [],
    ///     "spamSignals": [],
    ///     "detectedEntities": [],
    ///     "suggestedTags": ["Reliable", "High Quality", "Good Communication"],
    ///     "strengths": ["Reliable", "High Quality"],
    ///     "weaknesses": [],
    ///     "recommendation": 0,
    ///     "detectedKeywords": ["communication", "quality"],
    ///     "suggestions": [],
    ///     "maskedReview": null,
    ///     "processingTime": 182,
    ///     "cached": true,
    ///     "promptVersion": "1.0.0",
    ///     "model": "meta.llama4-scout-17b-instruct-v1:0"
    ///   }
    /// }
    /// </code>
    ///
    /// Example error response (400): invalid input returns a <c>Validation.Failed</c>
    /// error together with per-field messages.
    /// <code>
    /// {
    ///   "isSuccess": false,
    ///   "error": {
    ///     "code": "Validation.Failed",
    ///     "message": "Review text is required.",
    ///     "statusCode": 400,
    ///     "validationErrors": {
    ///       "reviewText": ["Review text is required."]
    ///     }
    ///   }
    /// }
    /// </code>
    /// </remarks>
    /// <param name="request">The marketplace review to moderate.</param>
    /// <param name="ct">Cancellation token propagated to the moderation pipeline.</param>
    /// <returns>
    /// 200 with the moderation verdict; 400 for invalid input; 401/403 when the
    /// caller is not authenticated or authorized; 404 when a referenced resource
    /// does not exist; 408 when the request times out; 429 when rate limited;
    /// 500 only for unexpected server failures (never for a moderation decision).
    /// </returns>
    [HttpPost("moderate")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ReviewModerationApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ModerateReview([FromBody] ReviewModerationRequest request, CancellationToken ct)
    {
        var validation = Validate(request);
        if (validation is not null)
            return HandleResult(validation);

        try
        {
            var response = await reviewModerationService.ModerateAsync(request, ct);
            return HandleResult(ApiResponse.Success(response.ToApiResponse()));
        }
        catch (Exception ex)
        {
            return HandleResult(ApiResponse.Failure<ReviewModerationApiResponse>(AppError.Failure(ex)));
        }
    }

    private static ApiResponse<ReviewModerationApiResponse>? Validate(ReviewModerationRequest request)
    {
        if (!ReviewModerationValidator.IsValid(request, out var error))
        {
            return ValidationFailure(
                request is null ? "request" : ValidationField(error!),
                error!);
        }

        if (request.ReviewId is null && request.ProjectId is null &&
            request.ReviewerId is null && request.ReviewedUserId is null)
        {
            const string message =
                "At least one of 'reviewId', 'projectId', 'reviewerId', or 'reviewedUserId' is required.";
            return ValidationFailure("reviewId", message);
        }

        if (!string.IsNullOrWhiteSpace(request.Language) && !LanguageHintRegex.IsMatch(request.Language))
        {
            return ValidationFailure(
                "language",
                "The 'language' field must be a valid language hint such as 'en' or 'ar-SA'.");
        }

        return null;
    }

    private static ApiResponse<ReviewModerationApiResponse> ValidationFailure(string field, string message)
        => ApiResponse.Failure<ReviewModerationApiResponse>(
            AppError.Validation(message, new Dictionary<string, string[]> { [field] = [message] }));

    private static string ValidationField(string error) => error switch
    {
        "The review request is required." => "request",
        "Review text is required." => "reviewText",
        _ when error.StartsWith("Review text exceeds", StringComparison.Ordinal) => "reviewText",
        _ => "rating"
    };

    /// <summary>
    /// Accepts a language hint shaped like a BCP 47 tag (for example "en", "ar",
    /// "en-US", "ar-SA"). The AI layer treats the hint as free-text context, so
    /// this is a format guard rather than an allow-list.
    /// </summary>
    private static readonly Regex LanguageHintRegex = new(
        @"^[A-Za-z]{2,8}(?:-[A-Za-z0-9]{1,8})*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
