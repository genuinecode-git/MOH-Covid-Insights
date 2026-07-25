using System.Text.Json;

namespace MohCovidInsights.Infrastructure.Parsing;

/// <summary>
/// Reads the CKAN datastore_search response:
/// <c>{ "result": { "fields": [{ "id": ... }], "records": [{ ... }], "total": n } }</c>
/// Multiple pages assemble into one table.
/// </summary>
public sealed class JsonTable : ITabularData
{
    private const string RowIdField = "_id";

    private JsonTable(IReadOnlyList<string> fields, IReadOnlyList<string[]> rows)
    {
        Fields = fields;
        Rows = rows;
    }

    public IReadOnlyList<string> Fields { get; }
    public IReadOnlyList<string[]> Rows { get; }

    public int IndexOf(string field)
    {
        for (var i = 0; i < Fields.Count; i++)
            if (Fields[i].Equals(field, StringComparison.OrdinalIgnoreCase))
                return i;

        return -1;
    }

    public int Require(string field)
    {
        var index = IndexOf(field);

        return index >= 0
            ? index
            : throw new InvalidOperationException(
                $"Field '{field}' not found. Available: {string.Join(", ", Fields)}");
    }

    public static JsonTable Load(string json) => Load([json]);

    public static JsonTable Load(IReadOnlyList<string> pages)
    {
        if (pages.Count == 0)
            throw new InvalidOperationException("No pages supplied.");

        List<string>? fields = null;
        var rows = new List<string[]>();

        foreach (var page in pages)
        {
            using var document = JsonDocument.Parse(page);

            if (!document.RootElement.TryGetProperty("result", out var result))
                throw new InvalidOperationException("Response has no 'result' property.");

            fields ??= ReadFields(result);

            if (!result.TryGetProperty("records", out var records)) continue;

            foreach (var record in records.EnumerateArray())
            {
                var row = new string[fields.Count];

                for (var i = 0; i < fields.Count; i++)
                    row[i] = record.TryGetProperty(fields[i], out var value)
                        ? ReadScalar(value)
                        : string.Empty;

                rows.Add(row);
            }
        }

        if (fields is null)
            throw new InvalidOperationException("Response contained no field definitions.");

        return new JsonTable(fields, rows);
    }

    private static List<string> ReadFields(JsonElement result)
    {
        if (!result.TryGetProperty("fields", out var fields))
            throw new InvalidOperationException("Response has no 'fields' array.");

        return fields.EnumerateArray()
            .Select(f => f.GetProperty("id").GetString() ?? string.Empty)
            .Where(id => id.Length > 0 && id != RowIdField)
            .ToList();
    }

    private static string ReadScalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        _ => value.GetRawText(),
    };
}