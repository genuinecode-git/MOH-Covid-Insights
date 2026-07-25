namespace MohCovidInsights.Application.Abstractions.Interfaces;

public interface IDatasetCatalogue
{
    Task<IReadOnlyList<DatasetCoverageInfo>> GetCoverageAsync(CancellationToken ct = default);
}
