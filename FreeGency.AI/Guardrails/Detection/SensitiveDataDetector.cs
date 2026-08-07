using System.Text.RegularExpressions;

namespace FreeGency.AI.Guardrails.Detection;

/// <summary>
/// Deterministic sensitive-data detector. Covers PII and secrets: emails, phone
/// numbers, credit cards (Luhn-validated), IBANs, bank accounts, crypto wallets,
/// private keys, passwords, OTPs, API keys, access tokens, JWTs, passport
/// numbers, national IDs, and driver licenses. Only the kind and match count are
/// recorded; matched values are never stored.
/// </summary>
public sealed class SensitiveDataDetector : IGuardrailDetector
{
    private static readonly SensitivePattern[] Patterns = BuildPatterns();

    /// <inheritdoc />
    public string Name => "SensitiveData";

    /// <inheritdoc />
    public bool IsEnabled(GuardrailOptions options) => options.EnableSensitiveData;

    /// <inheritdoc />
    public void Analyze(string text, GuardrailDetectorContext context, GuardrailResultBuilder builder)
    {
        foreach (var pattern in Patterns)
        {
            var count = 0;
            foreach (Match match in pattern.Regex.Matches(text))
            {
                if (!IsValidMatch(pattern.Kind, match))
                    continue;

                count++;
            }

            if (count > 0)
                builder.AddSensitive(pattern.Kind, count);
        }
    }

    private static bool IsValidMatch(SensitiveDataKind kind, Match match)
    {
        switch (kind)
        {
            case SensitiveDataKind.PhoneNumber:
                return CountDigits(match.Value) is >= 7 and <= 15;
            case SensitiveDataKind.CreditCard:
                return IsValidLuhn(DigitsOnly(match.Value));
            default:
                return match.Length > 0;
        }
    }

    private static string DigitsOnly(string value)
    {
        var sb = new System.Text.StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch is >= '0' and <= '9')
                sb.Append(ch);
        }

        return sb.ToString();
    }

    private static int CountDigits(string value)
    {
        var count = 0;
        foreach (var ch in value)
        {
            if (ch is >= '0' and <= '9')
                count++;
        }

        return count;
    }

    private static bool IsValidLuhn(string digits)
    {
        var sum = 0;
        var doubleDigit = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleDigit)
            {
                d *= 2;
                if (d > 9)
                    d -= 9;
            }

            sum += d;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    private static SensitivePattern[] BuildPatterns()
    {
        var raw = new (string RegexPattern, SensitiveDataKind Kind)[]
        {
            (@"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b", SensitiveDataKind.Email),
            (@"\+?\d[\d\s().\-]{5,16}\d", SensitiveDataKind.PhoneNumber),
            (@"\d[\d\s\-]{11,17}\d", SensitiveDataKind.CreditCard),
            (@"\b[A-Z]{2}\d{2}[A-Z0-9]{11,30}\b", SensitiveDataKind.Iban),
            (@"\b(?:account\s+number|bank\s+account|account\s+no)\b[^\n.]{0,25}\d{6,20}", SensitiveDataKind.BankAccount),
            (@"\b(?:bc1|[13])[A-HJ-NP-Za-km-z1-9]{25,39}\b", SensitiveDataKind.WalletAddress),
            (@"\b0x[a-fA-F0-9]{40}\b", SensitiveDataKind.WalletAddress),
            (@"-----BEGIN [A-Z0-9 ]*PRIVATE KEY-----", SensitiveDataKind.PrivateKey),
            (@"\b0x[a-fA-F0-9]{64}\b", SensitiveDataKind.PrivateKey),
            (@"\b(?:password|passwd|pwd|secret|كلمة المرور|الرقم السري)\b\s*(?::|=|is\s+|هو\s+)\S+", SensitiveDataKind.Password),
            (@"\b(?:otp|one[\s\-]?time\s+code|verification\s+code|رمز التحقق|كود التفعيل)\b[^\n.]{0,20}\d{4,8}", SensitiveDataKind.Otp),
            (@"\b(?:sk-[A-Za-z0-9]{16,}|pk-[A-Za-z0-9]{16,}|AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{20,}|xox[bap]-[A-Za-z0-9\-]{10,}|AIza[0-9A-Za-z\-]{20,}|key-[A-Za-z0-9]{20,}|token-[A-Za-z0-9]{20,})\b", SensitiveDataKind.ApiKey),
            (@"\b(?:api\s*key|api\s+token|مفتاح api)\b[^\n.]{0,25}\S{16,}", SensitiveDataKind.ApiKey),
            (@"\b(?:access\s+token|bearer|github\s+token|gitlab\s+token|توكن الوصول)\b[^\n.]{0,25}\S{12,}", SensitiveDataKind.AccessToken),
            (@"\beyJ[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}\b", SensitiveDataKind.Jwt),
            (@"\b(?:passport(?: number)?|رقم الجواز)\b[^\n.]{0,15}[A-Z0-9]{5,10}", SensitiveDataKind.PassportNumber),
            (@"\b[0-9]{14}\b", SensitiveDataKind.NationalId),
            (@"\b(?:national\s+id|national\s+identity|الرقم القومي|الهوية)\b[^\n.]{0,15}\d{9,15}", SensitiveDataKind.NationalId),
            (@"\b(?:driver'?s\s+license|driving\s+license|رخصة القيادة)\b[^\n.]{0,15}\S{4,}", SensitiveDataKind.DriverLicense)
        };

        return raw
            .Select(r => new SensitivePattern(new Regex(r.RegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase), r.Kind))
            .ToArray();
    }

    private sealed record SensitivePattern(Regex Regex, SensitiveDataKind Kind);
}
