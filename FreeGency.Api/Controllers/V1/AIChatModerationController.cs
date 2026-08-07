using System.Text;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.RateLimiting;
using FreeGency.AI.ChatModeration.Security;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Results;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FreeGency.Api.Controllers.V1;

/// <summary>
/// AI Chat Security API (Part 7).
///
/// Exposes the <c>POST /api/v1/ai/chat/moderate</c> endpoint that runs the
/// fully instrumented moderation pipeline (Parts 4 + 5 + 6) over a chat
/// message and returns a structured verdict.
///
/// The design is intentionally surface-agnostic: the same request/response
/// contract and rate limiter can later power reviews, comments, proposals,
/// project descriptions and profiles without any architectural change.
/// </summary>
[Route("api/v1/ai/chat")]
[Authorize]
public class AIChatModerationController(
    IChatModerationService chatModerationService,
    ChatModerationRateLimiter rateLimiter,
    IOptions<ChatModerationRateLimitOptions> rateLimitOptions,
    ICurrentUserService currentUserService) : BaseApiController
{
    /// <summary>The maximum accepted message length after normalization.</summary>
    private const int MaxMessageLength = 10_000;

    /// <summary>
    /// Moderates a chat message and returns whether it is safe to publish.
    ///
    /// The message is trimmed and Unicode-normalized (FormKC) before the AI
    /// pipeline runs. Empty messages and messages longer than
    /// <see cref="MaxMessageLength"/> characters are rejected with a 400.
    /// Requests are rate limited per user and per conversation; exceeding the
    /// per-minute limit returns a 429 with a <c>Retry-After</c> header.
    ///
    /// Example request:
    /// <code>
    /// {
    ///   "message": "Hello! Please check my portfolio.",
    ///   "conversationId": "a7f9c3b2-7c4e-4f8a-9d1e-2b3c4d5e6f70",
    ///   "projectId": "9e1a8b2c-5d4e-4c6b-8f0a-1b2c3d4e5f60",
    ///   "language": "en"
    /// }
    /// </code>
    ///
    /// Example response (200):
    /// <code>
    /// {
    ///   "isSuccess": true,
    ///   "data": {
    ///     "isSafe": true,
    ///     "riskScore": 8,
    ///     "confidence": 0.97,
    ///     "riskLevel": "Safe",
    ///     "action": "Allow",
    ///     "reason": "The message is polite and contains no risky content.",
    ///     "detectedLanguages": [ "en" ],
    ///     "processingTimeMs": 312,
    ///     "modelName": "meta.llama4-scout-17b-instruct-v1:0",
    ///     "usedCache": true,
    ///     "timestamp": "2026-08-02T10:15:30.0000000+00:00"
    ///   }
    /// }
    /// </code>
    /// </summary>
    /// <param name="request">The chat message to moderate.</param>
    /// <param name="ct">Cancellation token propagated to the moderation pipeline.</param>
    /// <returns>
    /// 200 with the moderation verdict; 400 for invalid input; 401/403 when the
    /// caller is not authenticated or authorized; 429 when rate limited;
    /// 500 only for unexpected server failures (never for a moderation decision).
    /// </returns>
    [HttpPost("moderate")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ChatModerationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Moderate([FromBody] ChatModerationRequest request, CancellationToken ct)
    {
        var normalized = Normalize(request);

        var validation = Validate(normalized);
        if (validation is not null)
            return HandleResult(validation);

        var rateLimit = rateLimiter.Check(
            BuildUserKey(currentUserService.UserId),
            BuildConversationKey(request.ConversationId),
            DateTimeOffset.UtcNow);

        if (!rateLimit.IsAllowed)
        {
            Response.Headers.RetryAfter = rateLimit.RetryAfter.TotalSeconds.ToString("0");
            return HandleResult(ApiResponse.Failure<ChatModerationResponse>(
                new AppError(
                    "RateLimit.Exceeded",
                    $"Too many moderation requests. Please retry after {rateLimit.RetryAfter.TotalSeconds:0} seconds.",
                    StatusCodes.Status429TooManyRequests)));
        }

        try
        {
            var response = await chatModerationService.ModerateAsync(normalized, ct);
            return HandleResult(ApiResponse.Success(response));
        }
        catch (Exception ex)
        {
            return HandleResult(ApiResponse.Failure<ChatModerationResponse>(AppError.Failure(ex)));
        }
    }

    private static ChatModerationRequest Normalize(ChatModerationRequest request)
    {
        var message = request.Message.Trim().Normalize(NormalizationForm.FormKC);
        var previousMessages = request.PreviousMessages
            .Select(previous => previous.Trim().Normalize(NormalizationForm.FormKC))
            .ToList();

        return request with { Message = message, PreviousMessages = previousMessages };
    }

    private static ApiResponse<ChatModerationResponse>? Validate(ChatModerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return ApiResponse.Failure<ChatModerationResponse>(
                AppError.Validation(
                    "The message cannot be empty.",
                    new Dictionary<string, string[]> { ["message"] = ["The 'message' field is required."] }));
        }

        if (request.Message.Length > MaxMessageLength)
        {
            return ApiResponse.Failure<ChatModerationResponse>(
                AppError.Validation(
                    $"The message must not exceed {MaxMessageLength} characters.",
                    new Dictionary<string, string[]> { ["message"] = [$"Maximum allowed length is {MaxMessageLength} characters."] }));
        }

        return null;
    }

    private static string BuildUserKey(Guid userId)
        => $"user:{userId}";

    private static string BuildConversationKey(string? conversationId)
        => $"conversation:{conversationId ?? "global"}";
}
