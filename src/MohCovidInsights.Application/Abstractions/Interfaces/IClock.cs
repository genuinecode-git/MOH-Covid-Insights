namespace MohCovidInsights.Application.Abstractions.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
