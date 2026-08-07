using System.Text.RegularExpressions;

namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic advertisement detector: external contact channels (Discord,
/// Telegram, WhatsApp, Facebook, Instagram, LinkedIn, websites), referral codes,
/// and coupon codes. Reuses the same external-channel signals as the review
/// content masker so results are consistent across modules.
/// </summary>
public sealed class AdvertisementDetector : IGuardrailDetector
{
    private static readonly Regex HandleUrlRegex = new(
        @"\b(?:wa\.me|t\.me|telegram\.me|discord\.(?:gg|com)|whatsapp\.com|facebook\.com|fb\.com|instagram\.com|linkedin\.com|github\.com|bit\.ly|shorturl\.at)/[^\s<>""]*",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HttpUrlRegex = new(
        @"https?://[^\s<>""]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ReferralPattern = new(
        @"\b(?:ref|referral|invite|code|coupon|promo|discount)[^a-z0-9]{0,6}[A-Z0-9]{4,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string[] ChannelMarkers =
    {
        "@discord", "@telegram", "@whatsapp", "@facebook", "@instagram", "@linkedin", "@tiktok",
        "discord.gg", "t.me/", "wa.me/", "add me on", "join my discord", "join our discord",
        "dm me", "message me on", "contact me on", "follow me on", "check my profile",
        "buy from me", "order now", "limited offer", "special offer", "hurry up", "only today",
        "اضفني", "أضفني", "تواصل معي", "راسلني", "تابعني", "انضم لقناتنا", "قناتي على",
        "بوتلنا", "صفحتي", "من صفحتي", "اشتر الآن", "عرض محدود", "خصم خاص", "كوبون", "كود خصم"
    };

    /// <inheritdoc />
    public string Name => "Advertisement";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableAdvertisement;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        if (HandleUrlRegex.IsMatch(text) || HttpUrlRegex.IsMatch(text))
        {
            builder.MarkAdvertisement();
            return;
        }

        var lowered = text.ToLowerInvariant();
        if (ChannelMarkers.Any(marker => lowered.Contains(marker, StringComparison.Ordinal)))
        {
            builder.MarkAdvertisement();
            return;
        }

        if (ReferralPattern.IsMatch(text))
            builder.MarkAdvertisement();
    }
}
