namespace MohCovidInsights.Domain.ValueObjects;

/// <summary>
/// Age bands used by the MOH hospitalisation dataset. Stored as the observation
/// dimension, so the raw CSV label is normalised to a stable key on ingestion.
/// </summary>
public static class AgeGroup
{
    public const string All = "all";
    public const string Children = "0-11";
    public const string Adults = "12-59";
    public const string Seniors = "60+";

    public static readonly IReadOnlyList<string> Known = [All, Children, Adults, Seniors];

    public static string Normalise(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return All;

        var compact = new string(raw.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        return compact switch
        {
            _ when compact.Contains("011") => Children,
            _ when compact.Contains("1259") => Adults,
            _ when compact.Contains("60") => Seniors,
            _ when compact.Contains("all") => All,
            _ => raw.Trim(),
        };
    }

    public static string Display(string key) => key switch
    {
        All => "All ages",
        Children => "0 - 11 years",
        Adults => "12 - 59 years",
        Seniors => "60 years and above",
        _ => key,
    };
}