using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MohCovidInsights.Api.IntegrationTests;

/// <summary>Builds CKAN datastore_search response bodies and registers them on a WireMock server.</summary>
public static class CkanStub
{
    public static string Body(IReadOnlyList<string> fields, IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        var fieldJson = string.Join(",", fields.Select(f => $$"""{ "id": "{{f}}" }"""));

        var recordJson = string.Join(",", rows.Select(row =>
        {
            var pairs = fields.Select((f, i) =>
            {
                var v = row[i];
                var literal = v switch
                {
                    null => "null",
                    string s => $"\"{s}\"",
                    _ => v.ToString(),
                };
                return $"\"{f}\": {literal}";
            });
            return "{" + string.Join(",", pairs) + "}";
        }));

        return $$"""
            { "success": true, "result": {
                "fields": [{{fieldJson}}],
                "records": [{{recordJson}}],
                "total": {{rows.Count}} } }
            """;
    }

    public static void Register(WireMockServer server, string datasetId, string body) =>
        server.Given(Request.Create()
                .WithPath("/api/action/datastore_search")
                .WithParam("resource_id", datasetId)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithHeader("Content-Type", "application/json")
                .WithBody(body));
}