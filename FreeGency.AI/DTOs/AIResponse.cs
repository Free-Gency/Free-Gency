namespace FreeGency.AI.DTOs;

public sealed class AIResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? RawOutput { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Elapsed { get; init; }
    public bool FromCache { get; init; }

    public static AIResponse<T> Success(T data, string? rawOutput = null, TimeSpan elapsed = default, bool fromCache = false)
        => new() { IsSuccess = true, Data = data, RawOutput = rawOutput, Elapsed = elapsed, FromCache = fromCache };

    public static AIResponse<T> Failure(string error)
        => new() { IsSuccess = false, ErrorMessage = error };
}
