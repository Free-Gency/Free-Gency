namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic toxicity detector: personal attacks, bullying, harassment,
/// violence, threats, extremism, sexism, racism, religious hate, and political
/// hate. English, Arabic, and Franco Arabic. Uses phrases for ambiguous English
/// terms (to limit false positives) and word-level signals for strong terms.
/// </summary>
public sealed class ToxicityDetector : IGuardrailDetector
{
    private static readonly ToxicityPattern[] Patterns = BuildPatterns();
    private static readonly TokenCategory[] StrongTokens = BuildStrongTokens();

    /// <inheritdoc />
    public string Name => "Toxicity";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableToxicity;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        var compact = GuardrailText.Compact(text);

        foreach (var pattern in Patterns)
        {
            if (compact.Contains(pattern.Compacted, StringComparison.Ordinal))
                builder.AddToxicity(pattern.Category);
        }

        var normalizedTokens = GuardrailText.Tokenize(text)
            .Select(GuardrailText.Normalize)
            .Where(t => t.Length >= 2)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var token in StrongTokens)
        {
            if (normalizedTokens.Contains(token.Normalized))
                builder.AddToxicity(token.Category);
        }
    }

    private static TokenCategory[] BuildStrongTokens()
    {
        var raw = new (string Token, ToxicityCategory Category)[]
        {
            // Strong English slurs
            ("nigger", ToxicityCategory.Racism),
            ("nigga", ToxicityCategory.Racism),
            ("retard", ToxicityCategory.PersonalAttack),
            ("dumbass", ToxicityCategory.PersonalAttack),
            ("faggot", ToxicityCategory.Racism),
            ("feminazi", ToxicityCategory.Sexism),
            // Arabic
            ("متخلف", ToxicityCategory.PersonalAttack),
            ("معتوه", ToxicityCategory.PersonalAttack),
            ("مريض", ToxicityCategory.PersonalAttack),
            ("حقير", ToxicityCategory.PersonalAttack),
            ("وضيع", ToxicityCategory.PersonalAttack),
            ("خنزير", ToxicityCategory.PersonalAttack),
            ("داعشي", ToxicityCategory.Extremism),
            ("كلب", ToxicityCategory.PersonalAttack)
        };

        return raw
            .Select(r => new TokenCategory(GuardrailText.Normalize(r.Token), r.Category))
            .Where(t => t.Normalized.Length >= 2)
            .ToArray();
    }

    private static ToxicityPattern[] BuildPatterns()
    {
        var raw = new (string Phrase, ToxicityCategory Category)[]
        {
            // English: personal attack / insult
            ("you are stupid", ToxicityCategory.PersonalAttack),
            ("you are an idiot", ToxicityCategory.PersonalAttack),
            ("you are dumb", ToxicityCategory.PersonalAttack),
            ("you are useless", ToxicityCategory.PersonalAttack),
            ("you are worthless", ToxicityCategory.PersonalAttack),
            ("you are pathetic", ToxicityCategory.PersonalAttack),
            ("you are a loser", ToxicityCategory.PersonalAttack),
            ("stupid piece of", ToxicityCategory.PersonalAttack),
            ("piece of garbage", ToxicityCategory.PersonalAttack),
            ("useless piece of", ToxicityCategory.PersonalAttack),
            ("shut up", ToxicityCategory.Harassment),
            ("shut your mouth", ToxicityCategory.Harassment),
            ("kiss my ass", ToxicityCategory.Harassment),
            ("suck my", ToxicityCategory.Harassment),
            ("piss off", ToxicityCategory.Harassment),
            ("go to hell", ToxicityCategory.Harassment),
            // English: bullying
            ("nobody likes you", ToxicityCategory.Bullying),
            ("everyone hates you", ToxicityCategory.Bullying),
            ("no one wants you", ToxicityCategory.Bullying),
            ("nobody wants you", ToxicityCategory.Bullying),
            ("you are a joke", ToxicityCategory.Bullying),
            ("pathetic loser", ToxicityCategory.Bullying),
            ("nobody cares about you", ToxicityCategory.Bullying),
            // English: violence / threat
            ("i will kill you", ToxicityCategory.Violence),
            ("i will beat you", ToxicityCategory.Violence),
            ("i will hurt you", ToxicityCategory.Violence),
            ("i will punch you", ToxicityCategory.Violence),
            ("i will slap you", ToxicityCategory.Violence),
            ("i will stab you", ToxicityCategory.Violence),
            ("i will shoot you", ToxicityCategory.Violence),
            ("beat you up", ToxicityCategory.Violence),
            ("kill yourself", ToxicityCategory.Violence),
            ("go die", ToxicityCategory.Violence),
            ("you should die", ToxicityCategory.Violence),
            ("you deserve to die", ToxicityCategory.Violence),
            ("you will regret", ToxicityCategory.Threat),
            ("you better watch out", ToxicityCategory.Threat),
            ("watch your back", ToxicityCategory.Threat),
            ("i know where you live", ToxicityCategory.Threat),
            ("i am coming for you", ToxicityCategory.Threat),
            ("you are dead", ToxicityCategory.Threat),
            ("dead meat", ToxicityCategory.Threat),
            // English: extremism / political hate
            ("bomb the", ToxicityCategory.Extremism),
            ("blow up the", ToxicityCategory.Extremism),
            ("kill all", ToxicityCategory.PoliticalHate),
            ("exterminate all", ToxicityCategory.PoliticalHate),
            ("death to", ToxicityCategory.PoliticalHate),
            ("kill them all", ToxicityCategory.PoliticalHate),
            // English: sexism / racism / religious hate
            ("women belong in", ToxicityCategory.Sexism),
            ("women belong in the kitchen", ToxicityCategory.Sexism),
            ("make me a sandwich", ToxicityCategory.Sexism),
            ("typical woman", ToxicityCategory.Sexism),
            ("go back to your country", ToxicityCategory.Racism),
            ("white trash", ToxicityCategory.Racism),
            ("islam is evil", ToxicityCategory.ReligiousHate),
            ("muslims are terrorists", ToxicityCategory.ReligiousHate),
            ("christians are", ToxicityCategory.ReligiousHate),
            ("jews are", ToxicityCategory.ReligiousHate),
            // Arabic: personal attack / harassment
            ("يا غبي", ToxicityCategory.PersonalAttack),
            ("يا أحمق", ToxicityCategory.PersonalAttack),
            ("يا حمار", ToxicityCategory.PersonalAttack),
            ("يا كلب", ToxicityCategory.PersonalAttack),
            ("سكت", ToxicityCategory.Harassment),
            ("اخرس", ToxicityCategory.Harassment),
            ("قفل فمك", ToxicityCategory.Harassment),
            ("سكر خشمك", ToxicityCategory.Harassment),
            // Arabic: violence / threat
            ("سأقتلك", ToxicityCategory.Violence),
            ("اقتلك", ToxicityCategory.Violence),
            ("أضربك", ToxicityCategory.Violence),
            ("أكسرك", ToxicityCategory.Violence),
            ("سوف أذبحك", ToxicityCategory.Violence),
            ("سأنهي عليك", ToxicityCategory.Violence),
            ("هتندم", ToxicityCategory.Threat),
            ("سأندمك", ToxicityCategory.Threat),
            ("اعرف عنوانك", ToxicityCategory.Threat),
            ("جبت لك", ToxicityCategory.Threat),
            ("خلصت عليك", ToxicityCategory.Threat),
            // Arabic: extremism / religious / political hate
            ("تفجير", ToxicityCategory.Extremism),
            ("داعش", ToxicityCategory.Extremism),
            ("الجهاد", ToxicityCategory.Extremism),
            ("سفك الدماء", ToxicityCategory.PoliticalHate),
            ("على وجه الأرض", ToxicityCategory.PoliticalHate),
            ("المرأة مكانها", ToxicityCategory.Sexism),
            ("الإسلام يسمح", ToxicityCategory.ReligiousHate),
            // Franco Arabic
            ("ya 7ayawan", ToxicityCategory.PersonalAttack),
            ("ya kalb", ToxicityCategory.PersonalAttack),
            ("yekhar", ToxicityCategory.Harassment),
            ("oskoos", ToxicityCategory.Harassment),
            ("ra7 adabnak", ToxicityCategory.Threat),
            ("hadjef", ToxicityCategory.Threat),
            ("n3ajmek", ToxicityCategory.Threat),
            ("haramik", ToxicityCategory.PersonalAttack)
        };

        return raw
            .Select(r => new ToxicityPattern(GuardrailText.Compact(r.Phrase), r.Category))
            .ToArray();
    }

    private sealed record ToxicityPattern(string Compacted, ToxicityCategory Category);
    private sealed record TokenCategory(string Normalized, ToxicityCategory Category);
}
