namespace FreeGency.Domain.Constants;

public static class Currencies
{
    public const string USD = "USD";
    public const string EUR = "EUR";
    public const string GBP = "GBP";
    public const string EGP = "EGP";
    public const string SAR = "SAR";
    public const string AED = "AED";
    public const string KWD = "KWD";
    public const string QAR = "QAR";
    public const string BHD = "BHD";
    public const string OMR = "OMR";
    public const string JOD = "JOD";

    public static readonly IReadOnlyCollection<string> Supported =
    [
        USD,
        EUR,
        GBP,
        EGP,
        SAR,
        AED,
        KWD,
        QAR,
        BHD,
        OMR,
        JOD
    ];
}