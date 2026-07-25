namespace MohCovidInsights.Infrastructure.Sync;

public sealed record SyncSummary(int Succeeded, int Skipped, int Failed, int TotalRows);
