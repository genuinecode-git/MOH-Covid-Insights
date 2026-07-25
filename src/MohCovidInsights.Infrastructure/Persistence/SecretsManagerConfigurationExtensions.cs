using Microsoft.Extensions.Configuration;

namespace MohCovidInsights.Infrastructure.Persistence;

public static class SecretsManagerConfigurationExtensions
{
    public const string SecretArnVariable = "DB_SECRET_ARN";

    public static IConfigurationBuilder AddRdsSecretIfPresent(this IConfigurationBuilder builder)
    {
        var arn = Environment.GetEnvironmentVariable(SecretArnVariable);
        return string.IsNullOrWhiteSpace(arn)
            ? builder
            : builder.Add(new SecretsManagerConfigurationSource(arn));
    }
}
