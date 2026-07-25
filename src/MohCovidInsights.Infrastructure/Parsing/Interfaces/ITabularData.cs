namespace MohCovidInsights.Infrastructure.Parsing.Interfaces;

/// <summary>
/// Column-addressable tabular data. Columns are addressed by their exact upstream
/// field id, so a schema change fails loudly rather than binding to a neighbour.
/// </summary>
public interface ITabularData
{
    IReadOnlyList<string> Fields { get; }
    IReadOnlyList<string[]> Rows { get; }

    /// <summary>Index of <paramref name="field"/>, or -1 when absent. Case-insensitive.</summary>
    int IndexOf(string field);

    /// <summary>As <see cref="IndexOf"/>, but throws listing the available fields.</summary>
    int Require(string field);
}
