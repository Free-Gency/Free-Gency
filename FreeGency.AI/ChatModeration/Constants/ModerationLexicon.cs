namespace FreeGency.AI.ChatModeration.Constants;

/// <summary>
/// Immutable profanity and abuse keywords used for local matching, masking,
/// and metrics. Used alongside the model's own judgment.
/// </summary>
public static class ModerationLexicon
{
    /// <summary>Arabic profanity.</summary>
    public static readonly IReadOnlyList<string> ArabicProfanity =
    [
        "كسم", "كسمك", "كس امك", "يا عرص", "يا ابن الوسخة", "خول", "شرموط", "متناك", "احا",
        "زب", "طيز", "كس", "يا حيوان", "يا غبي", "يا متخلف", "يا وسخ", "يا نجس",
        "يا ابن الكلب", "يا ابن الشرموطة"
    ];

    /// <summary>English profanity.</summary>
    public static readonly IReadOnlyList<string> EnglishProfanity =
    [
        "fuck", "fucking", "shit", "bitch", "asshole", "motherfucker", "retard",
        "moron", "loser", "idiot", "stupid", "bastard", "piece of shit"
    ];

    /// <summary>Franco Arabic profanity (Arabizi).</summary>
    public static readonly IReadOnlyList<string> FrancoProfanity =
    [
        "kosomk", "kos omak", "5awal", "7omar", "ya 7ayawan", "metnak", "sharmota", "3ars", "khawal"
    ];

    /// <summary>Emojis used to express abuse or hostility.</summary>
    public static readonly IReadOnlyList<string> AbusiveEmojis =
    [
        "🤬", "🖕", "💩", "💀", "🔪", "🔫"
    ];

    /// <summary>All profanity keywords combined (Arabic, English, and Franco Arabic).</summary>
    public static readonly IReadOnlyList<string> Profanity =
        [.. ArabicProfanity, .. EnglishProfanity, .. FrancoProfanity];
}
