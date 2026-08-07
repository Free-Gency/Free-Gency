namespace FreeGency.AI.ChatModeration.DTOs;

/// <summary>
/// Performance and usage statistics for a moderation run.
/// </summary>
/// <param name="ProcessingTime">The total processing time.</param>
/// <param name="TokenUsage">The total number of tokens consumed by the model call.</param>
/// <param name="PromptTokens">The number of prompt tokens consumed by the model call.</param>
/// <param name="CompletionTokens">The number of completion tokens produced by the model call.</param>
/// <param name="CacheHit">Whether the result was served from cache.</param>
public sealed record ModerationStatisticsDto(
    TimeSpan ProcessingTime,
    int TokenUsage,
    int PromptTokens,
    int CompletionTokens,
    bool CacheHit);
