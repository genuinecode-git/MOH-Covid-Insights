using Microsoft.Extensions.Configuration;

namespace MohCovidInsights.Infrastructure.Persistence;

public static class SecretsManagerConfigurationExtensions
{
    public const string SecretArnVariable = "DB_SECRET_ARN";
    public const string ReadOnlySecretArnVariable = "DB_READONLY_SECRET_ARN";

    public static IConfigurationBuilder AddRdsSecretIfPresent(this IConfigurationBuilder builder)
    {
        var defaultArn = Environment.GetEnvironmentVariable(SecretArnVariable);
        if (!string.IsNullOrWhiteSpace(defaultArn))
            builder.Add(new SecretsManagerConfigurationSource(defaultArn, "Default"));

        var readOnlyArn = Environment.GetEnvironmentVariable(ReadOnlySecretArnVariable);
        if (!string.IsNullOrWhiteSpace(readOnlyArn))
            builder.Add(new SecretsManagerConfigurationSource(readOnlyArn, "ReadOnly"));

        return builder;
    }
}
