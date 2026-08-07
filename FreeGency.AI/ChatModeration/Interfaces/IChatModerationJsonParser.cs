using FreeGency.AI.ChatModeration.Models;

namespace FreeGency.AI.ChatModeration.Interfaces;

public interface IChatModerationJsonParser
{
    bool TryParse(string? raw, out ModerationAiResult? result);
}
