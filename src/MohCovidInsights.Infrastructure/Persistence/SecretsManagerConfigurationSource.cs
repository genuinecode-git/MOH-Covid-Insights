using Microsoft.Extensions.Configuration;

namespace MohCovidInsights.Infrastructure.Persistence;

public sealed class SecretsManagerConfigurationSource(string secretArn, string connectionName) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new SecretsManagerConfigurationProvider(secretArn, connectionName);
}
