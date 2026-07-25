using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Application.Abstractions;

public sealed record DatasetCoverageInfo(
    string DatasetId,
    string Name,
    EpiWeek? From,
    EpiWeek? To,
    int RecordCount,
    DateTimeOffset? LastSyncedAt,
    string Tone);
