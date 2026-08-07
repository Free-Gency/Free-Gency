using System.Text;
using FreeGency.AI.Moderation;
using FreeGency.AI.Moderation.Enums;
using FreeGency.AI.Moderation.Models;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Results;
using Microsoft.AspNetCore.Http;

namespace FreeGency.Api.Controllers.V1;

/// <summary>
/// AI Moderation API (Part 8).
///
/// Exposes the <c>POST /api/v1/ai/moderation/moderate</c> endpoint that runs the
/// reusable, feature-agnostic moderation core (<see cref="IModerationService"/>)
/// over any user-generated content: chat messages, reviews, comments, project
/// descriptions, proposal cover letters, profiles, and support tickets.
///
/// The core is surface-agnostic and never throws to clients: empty or oversized
/// content is rejected with a 400, and any AI failure (timeout, circuit breaker,
/// repeated AI error) resolves to a manual-review result instead of a 500.
/// </summary>
[Route("api/v1/ai/moderation")]
[Authorize]
public class AIModerationController(
    IModerationService moderationService) : BaseApiController
{
    /// <summary>The maximum accepted content length after normalization.</summary>
    private const int MaxContentLength = 10_000;

    /// <summary>
    /// Moderates any user-generated content and returns a structured verdict.
    ///
    /// The content is trimmed and Unicode-normalized (FormKC) before the
    /// moderation pipeline runs. Empty content and content longer than
    /// <see cref="MaxContentLength"/> characters are rejected with a 400.
    ///
    /// Example request:
    /// <code>
    /// {
    ///   "content": "Contact me on WhatsApp +20 100 000 0000 for a guaranteed visa.",
    ///   "contentKind": "ChatMessage",
    ///   "context": "A freelancer is messaging a client about a design project.",
    ///   "metadata": {
    ///     "senderRole": "Freelancer",
    ///     "receiverRole": "Client"
    ///   }
    /// }
    /// </code>
    ///
    /// Example response (200):
    /// <code>
    /// {
    ///   "isSuccess": true,
    ///   "data": {
    ///     "success": true,
    ///     "analysis": {
    ///       "isSafe": false,
    ///       "riskScore": 72,
    ///       "confidence": 0.94,
    ///       "riskLevel": 3,
    ///       "action": 2,
    ///       "reason": "The content attempts to move the conversation off-platform.",
    ///       "categories": [
    ///         { "category": "ExternalContact", "score": 0.9 },
    ///         { "category": "Scam", "score": 0.7 }
    ///       ],
    ///       "promptVersion": "1.0",
    ///       "modelName": "meta.llama4-scout-17b-instruct-v1:0"
    ///     },
    ///     "error": null,
    ///     "fromCache": false,
    ///     "retryCount": 0,
    ///     "circuitBroken": false,
    ///     "elapsedMs": 312,
    ///     "timestamp": "2026-08-02T10:15:30.0000000+00:00"
    ///   }
    /// }
    /// </code>
    ///
    /// Enum values: <c>riskLevel</c> Safe=0 Low=1 Medium=2 High=3 Critical=4;
    /// <c>action</c> Allow=0 Warn=1 Mask=2 Reject=3 ManualReview=4 TemporaryMute=5 PermanentBan=6.
    /// </summary>
    /// <param name="request">The content to moderate.</param>
    /// <param name="ct">Cancellation token propagated to the moderation pipeline.</param>
    /// <returns>
    /// 200 with the moderation result; 400 for invalid input; 401/403 when the
    /// caller is not authenticated or authorized; 500 only for unexpected server
    /// failures (never for a moderation decision).
    /// </returns>
    [HttpPost("moderate")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ModerationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Moderate([FromBody] ModerationRequest request, CancellationToken ct)
    {
        var normalized = Normalize(request);

        var validation = Validate(normalized);
        if (validation is not null)
            return HandleResult(validation);

        try
        {
            var result = await moderationService.ModerateAsync(normalized, ct);
            return HandleResult(ApiResponse.Success(result));
        }
        catch (Exception ex)
        {
            return HandleResult(ApiResponse.Failure<ModerationResult>(AppError.Failure(ex)));
        }
    }

    private static ModerationRequest Normalize(ModerationRequest request)
    {
        return new ModerationRequest
        {
            Content = (request.Content ?? string.Empty).Trim().Normalize(NormalizationForm.FormKC),
            ContentKind = request.ContentKind,
            Context = string.IsNullOrWhiteSpace(request.Context)
                ? null
                : request.Context.Trim().Normalize(NormalizationForm.FormKC),
            Metadata = request.Metadata
        };
    }

    private static ApiResponse<ModerationResult>? Validate(ModerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ApiResponse.Failure<ModerationResult>(
                AppError.Validation(
                    "The content cannot be empty.",
                    new Dictionary<string, string[]> { ["content"] = ["The 'content' field is required."] }));
        }

        if (request.Content.Length > MaxContentLength)
        {
            return ApiResponse.Failure<ModerationResult>(
                AppError.Validation(
                    $"The content must not exceed {MaxContentLength} characters.",
                    new Dictionary<string, string[]> { ["content"] = [$"Maximum allowed length is {MaxContentLength} characters."] }));
        }

        return null;
    }
}
