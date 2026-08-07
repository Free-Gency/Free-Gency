using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Models;

namespace FreeGency.AI.ChatModeration.Interfaces;

public interface IChatModerationPromptBuilder
{
    ModerationPrompt Build(ModerationRequest request);
}
