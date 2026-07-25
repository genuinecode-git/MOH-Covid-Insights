using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

using MohCovidInsights.Ingestion;

await using var function = new Function();

if (Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API") is not null)
{
    await LambdaBootstrapBuilder
        .Create<object?>(function.HandleAsync, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();

    return 0;
}

try
{
    await function.HandleAsync(null);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
