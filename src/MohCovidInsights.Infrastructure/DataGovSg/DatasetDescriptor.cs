using System.ComponentModel.DataAnnotations;

namespace MohCovidInsights.Infrastructure.DataGovSg;

public sealed class DatasetDescriptor
{
    [Required] public string Id { get; init; } = null!;
    [Required] public string Name { get; init; } = null!;
    [Required] public string Parser { get; init; } = null!;

    /// <summary>Semantic tone the client maps to a colour.</summary>
    public string Tone { get; init; } = "neutral";
}