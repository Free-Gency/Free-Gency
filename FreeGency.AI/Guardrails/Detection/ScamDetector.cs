namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic scam detector: investment, crypto, advance-payment, gift-card,
/// employment, and refund scams. English, Arabic, and Franco Arabic. Phrase-based
/// on the compacted text so spacing and leet variations normalize away.
/// </summary>
public sealed class ScamDetector : IGuardrailDetector
{
    private static readonly ScamPattern[] Patterns = BuildPatterns();

    /// <inheritdoc />
    public string Name => "Scam";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableScam;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        var compact = GuardrailText.Compact(text);

        foreach (var pattern in Patterns)
        {
            if (compact.Contains(pattern.Compacted, StringComparison.Ordinal))
                builder.AddScam(pattern.Category);
        }
    }

    private static ScamPattern[] BuildPatterns()
    {
        var raw = new (string Phrase, ScamCategory Category)[]
        {
            // Investment scam
            ("guaranteed returns", ScamCategory.InvestmentScam),
            ("double your money", ScamCategory.InvestmentScam),
            ("passive income", ScamCategory.InvestmentScam),
            ("risk free investment", ScamCategory.InvestmentScam),
            ("get rich quick", ScamCategory.InvestmentScam),
            ("no risk investment", ScamCategory.InvestmentScam),
            ("guaranteed profit", ScamCategory.InvestmentScam),
            ("multi level marketing", ScamCategory.InvestmentScam),
            ("recruit more", ScamCategory.InvestmentScam),
            // Crypto scam
            ("investment in crypto", ScamCategory.CryptoScam),
            ("send crypto to", ScamCategory.CryptoScam),
            ("send bitcoin to", ScamCategory.CryptoScam),
            ("send usdt to", ScamCategory.CryptoScam),
            ("crypto giveaway", ScamCategory.CryptoScam),
            ("wallet address", ScamCategory.CryptoScam),
            ("double your bitcoin", ScamCategory.CryptoScam),
            ("deposit to my wallet", ScamCategory.CryptoScam),
            // Advance payment scam
            ("pay a deposit first", ScamCategory.AdvancePaymentScam),
            ("advance payment", ScamCategory.AdvancePaymentScam),
            ("pay before you receive", ScamCategory.AdvancePaymentScam),
            ("pay an advance fee", ScamCategory.AdvancePaymentScam),
            ("send the money first", ScamCategory.AdvancePaymentScam),
            ("western union", ScamCategory.AdvancePaymentScam),
            ("money gram", ScamCategory.AdvancePaymentScam),
            // Gift card scam
            ("gift card", ScamCategory.GiftCardScam),
            ("apple gift card", ScamCategory.GiftCardScam),
            ("google play gift card", ScamCategory.GiftCardScam),
            ("steam gift card", ScamCategory.GiftCardScam),
            ("scratch the card", ScamCategory.GiftCardScam),
            ("send the gift card code", ScamCategory.GiftCardScam),
            // Employment scam
            ("work from home and earn", ScamCategory.EmploymentScam),
            ("easy money online", ScamCategory.EmploymentScam),
            ("no experience needed", ScamCategory.EmploymentScam),
            ("earn money per review", ScamCategory.EmploymentScam),
            ("pay to get the job", ScamCategory.EmploymentScam),
            ("paid to post reviews", ScamCategory.EmploymentScam),
            // Refund scam
            ("refund your money", ScamCategory.RefundScam),
            ("claim your refund", ScamCategory.RefundScam),
            ("you have a refund", ScamCategory.RefundScam),
            ("to receive your refund", ScamCategory.RefundScam),
            ("pay a fee to get refund", ScamCategory.RefundScam),
            // Arabic
            ("أرباح مضمونة", ScamCategory.InvestmentScam),
            ("ضعف أموالك", ScamCategory.InvestmentScam),
            ("استثمر واكسب", ScamCategory.InvestmentScam),
            ("ثروة بسرعة", ScamCategory.InvestmentScam),
            ("اربح بسرعة", ScamCategory.InvestmentScam),
            ("أرسل عملة رقمية", ScamCategory.CryptoScam),
            ("أرسل بيتكوين", ScamCategory.CryptoScam),
            ("هدية عملات رقمية", ScamCategory.CryptoScam),
            ("ادفع مقدم", ScamCategory.AdvancePaymentScam),
            ("ادفع قبل الاستلام", ScamCategory.AdvancePaymentScam),
            ("تحويل مسبق", ScamCategory.AdvancePaymentScam),
            ("بطاقة هدايا", ScamCategory.GiftCardScam),
            ("كود البطاقة", ScamCategory.GiftCardScam),
            ("اشتر بطاقة", ScamCategory.GiftCardScam),
            ("اربح من المنزل", ScamCategory.EmploymentScam),
            ("وظيفة بدون خبرة", ScamCategory.EmploymentScam),
            ("اكتب مراجعات مقابل أجر", ScamCategory.EmploymentScam),
            ("ادفع للحصول على الوظيفة", ScamCategory.EmploymentScam),
            ("استرداد أموالك", ScamCategory.RefundScam),
            ("تحصيل مبلغ", ScamCategory.RefundScam),
            // Franco Arabic
            ("arbe7 b talon", ScamCategory.InvestmentScam),
            ("doubl money", ScamCategory.InvestmentScam),
            ("istamir we arbah", ScamCategory.InvestmentScam),
            ("sirif bitcoin", ScamCategory.CryptoScam),
            ("lighaybit", ScamCategory.CryptoScam),
            ("def3 moqadam", ScamCategory.AdvancePaymentScam),
            ("western union", ScamCategory.AdvancePaymentScam),
            ("gift card", ScamCategory.GiftCardScam),
            ("kteb review bel fous", ScamCategory.EmploymentScam)
        };

        return raw
            .Select(r => new ScamPattern(GuardrailText.Compact(r.Phrase), r.Category))
            .ToArray();
    }

    private sealed record ScamPattern(string Compacted, ScamCategory Category);
}
