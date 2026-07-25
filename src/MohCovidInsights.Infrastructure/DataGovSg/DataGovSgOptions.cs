using System.ComponentModel.DataAnnotations;

namespace MohCovidInsights.Infrastructure.DataGovSg;

public sealed class DataGovSgOptions
{
    public const string SectionName = "DataGovSg";

    [Required]
    public string BaseUrl { get; init; } = "https://data.gov.sg/";

    public string CollectionId { get; init; } = "522";

    /// <summary>Records per request. CKAN caps this; 1000 covers every dataset here in one call.</summary>
    public int PageSize { get; init; } = 1000;

    /// <summary>Safety valve against an unbounded pagination loop.</summary>
    public int MaxPages { get; init; } = 20;

    /// <summary>Pause between datasets. data.gov.sg permits roughly 5 requests per minute.</summary>
    public TimeSpan DatasetDelay { get; init; } = TimeSpan.FromSeconds(8);

    [Required, MinLength(1)]
    public IReadOnlyList<DatasetDescriptor> Datasets { get; init; } = [];
}