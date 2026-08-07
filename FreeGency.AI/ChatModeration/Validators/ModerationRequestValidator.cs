using FreeGency.AI.ChatModeration.DTOs;

namespace FreeGency.AI.ChatModeration.Validators;

public sealed class ModerationRequestValidator
{
    private readonly ChatModerationOptions _options;

    public ModerationRequestValidator(ChatModerationOptions options)
    {
        _options = options;
    }

    public bool Validate(ModerationRequest request, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            error = "Content is required.";
            return false;
        }

        if (request.Content.Length < _options.MinContentLength)
        {
            error = $"Content is shorter than the minimum of {_options.MinContentLength} characters.";
            return false;
        }

        if (request.Content.Length > _options.MaxContentLength)
        {
            error = $"Content exceeds the maximum of {_options.MaxContentLength} characters.";
            return false;
        }

        return true;
    }
}
