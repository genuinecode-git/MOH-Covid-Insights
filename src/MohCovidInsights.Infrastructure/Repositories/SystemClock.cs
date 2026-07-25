using MohCovidInsights.Application.Abstractions.Interfaces;

namespace MohCovidInsights.Infrastructure.Repositories;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
