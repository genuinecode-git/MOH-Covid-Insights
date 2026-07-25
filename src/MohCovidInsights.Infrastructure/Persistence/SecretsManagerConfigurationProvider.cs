using System.Text.Json;
using System.Text.Json.Serialization;

using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

using Microsoft.Extensions.Configuration;

using Npgsql;

namespace MohCovidInsights.Infrastructure.Persistence;

public sealed class SecretsManagerConfigurationProvider(string secretArn) : ConfigurationProvider
{
    public override void Load() => LoadAsync().GetAwaiter().GetResult();

    private async Task LoadAsync()
    {
        using var client = new AmazonSecretsManagerClient();
        var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretArn });

        var secret = JsonSerializer.Deserialize<RdsSecret>(response.SecretString)
                     ?? throw new InvalidOperationException($"Secret {secretArn} could not be parsed.");

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = secret.Host,
            Port = secret.Port,
            Database = secret.DbName,
            Username = secret.Username,
            Password = secret.Password,
            SslMode = SslMode.Require,
            Pooling = true,
            MaxPoolSize = 5,
            Timeout = 10,
            CommandTimeout = 20,
        }.ConnectionString;

        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ConnectionStrings:Default"] = connectionString,
        };
    }

    private sealed record RdsSecret
    {
        [JsonPropertyName("username")] 
        public string Username { get; init; } = null!;
        [JsonPropertyName("password")] 
        public string Password { get; init; } = null!;
        [JsonPropertyName("host")] 
        public string Host { get; init; } = null!;
        [JsonPropertyName("port")] 
        public int Port { get; init; } = 5432;
        [JsonPropertyName("dbname")] 
        public string DbName { get; init; } = "mohcovid";
    }
}
