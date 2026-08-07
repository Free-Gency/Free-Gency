namespace FreeGency.AI.ChatModeration.Enums;

/// <summary>
/// The dominant language detected in a moderated message.
/// </summary>
public enum ContentLanguage
{
    /// <summary>The language could not be determined.</summary>
    Unknown = 0,

    /// <summary>Arabic script.</summary>
    Arabic = 1,

    /// <summary>English or Latin-script text.</summary>
    English = 2,

    /// <summary>Franco Arabic (Arabizi): Arabic written with Latin letters and digits.</summary>
    Franco = 3,

    /// <summary>A mixture of Arabic and Latin scripts in the same message.</summary>
    Mixed = 4,

    /// <summary>The message consists mostly of emojis or symbols.</summary>
    Emoji = 5,

    /// <summary>The message resembles source code.</summary>
    Programming = 6
}
