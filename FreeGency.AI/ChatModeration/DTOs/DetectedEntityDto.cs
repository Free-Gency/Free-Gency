using FreeGency.AI.ChatModeration.Enums;

namespace FreeGency.AI.ChatModeration.DTOs;

/// <summary>
/// A piece of sensitive or personally identifiable data detected inside a message.
/// </summary>
/// <param name="Type">The kind of entity that was detected.</param>
/// <param name="Value">The detected raw value.</param>
/// <param name="StartIndex">The zero-based start index of the match in the original message.</param>
/// <param name="EndIndex">The zero-based end index (exclusive) of the match in the original message.</param>
/// <param name="Confidence">The detection confidence on a 0..1 scale.</param>
public sealed record DetectedEntityDto(
    DetectedEntityType Type,
    string Value,
    int StartIndex,
    int EndIndex,
    double Confidence);
