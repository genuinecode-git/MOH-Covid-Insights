using MohCovidInsights.Domain.ValueObjects;
using MohCovidInsights.Infrastructure.Parsing;
using Shouldly;

namespace MohCovidInsights.Infrastructure.Tests;

public class ParserTests
{
    private static JsonTable Fixture(string name) =>
        JsonTable.Load(File.ReadAllText(Path.Combine("Fixtures", $"{name}.json")));

    // ---- Specs validated against captured upstream responses ----

    public static TheoryData<string, LongFormatSpec> Specs => new()
    {
        { "infections", DatasetSpecs.Infections },
        { "admissions", DatasetSpecs.Admissions },
        { "average-cases", DatasetSpecs.AverageCases },
        { "icu-beds", DatasetSpecs.IcuBeds },
    };

    [Theory]
    [MemberData(nameof(Specs))]
    public void EverySpec_MatchesItsRealDataset(string fixture, LongFormatSpec spec)
    {
        var result = new LongFormatParser(spec).Parse(Fixture(fixture));

        result.ShouldNotBeEmpty($"'{fixture}' produced no observations — the spec has drifted.");
        result.ShouldAllBe(o => o.EpiWeek.IsInitialised);
    }

    // ---- Shape-specific behaviour ----

    [Fact]
    public void Infections_ReadsASingleMetricWithNoStatusField()
    {
        const string json = """
            { "result": {
                "fields": [{ "id": "epi_year" }, { "id": "epi_week" }, { "id": "est_count" }],
                "records": [
                  { "epi_year": "2023", "epi_week": "2023-49", "est_count": "56043" },
                  { "epi_year": "2023", "epi_week": "2023-50", "est_count": "58300" }
                ], "total": 2 } }
            """;

        var result = new LongFormatParser(DatasetSpecs.Infections).Parse(JsonTable.Load(json));

        result.Count.ShouldBe(2);
        result[1].EpiWeek.ShouldBe(new EpiWeek(2023, 50));
        result[1].Value.ShouldBe(58300m);
        result[1].MetricCode.ShouldBe(MetricCode.EstimatedInfections.Value);
    }

    [Fact]
    public void Admissions_MapsStatusValuesToMetrics()
    {
        const string json = """
            { "result": {
                "fields": [{ "id": "epi_year" }, { "id": "epi_week" },
                           { "id": "new_admisison_type" }, { "id": "count" }],
                "records": [
                  { "epi_year": "2023", "epi_week": "2023-09", "new_admisison_type": "Hospitalised", "count": "84" },
                  { "epi_year": "2023", "epi_week": "2023-09", "new_admisison_type": "ICU", "count": "3" }
                ], "total": 2 } }
            """;

        var result = new LongFormatParser(DatasetSpecs.Admissions).Parse(JsonTable.Load(json));

        result.Single(o => o.MetricCode == MetricCode.HospitalAdmissions.Value).Value.ShouldBe(84m);
        result.Single(o => o.MetricCode == MetricCode.IcuAdmissions.Value).Value.ShouldBe(3m);
    }

    [Fact]
    public void AverageCases_SplitsByStatusAndAgeGroup()
    {
        const string json = """
            { "result": {
                "fields": [{ "id": "epi_year" }, { "id": "epi_week" },
                           { "id": "clinical_status" }, { "id": "age_groups" }, { "id": "count" }],
                "records": [
                  { "epi_year": "2023", "epi_week": "2023-09", "clinical_status": "Hospitalised", "age_groups": "0 - 11 years old", "count": "1.7" },
                  { "epi_year": "2023", "epi_week": "2023-09", "clinical_status": "Hospitalised", "age_groups": "12 - 59 years old", "count": "6.9" },
                  { "epi_year": "2023", "epi_week": "2023-09", "clinical_status": "Hospitalised", "age_groups": "60 years old and above", "count": "34.9" }
                ], "total": 3 } }
            """;

        var result = new LongFormatParser(DatasetSpecs.AverageCases).Parse(JsonTable.Load(json));

        result.Single(o => o.Dimension == AgeGroup.Seniors).Value.ShouldBe(34.9m);
        result.Single(o => o.Dimension == AgeGroup.All).Value.ShouldBe(43.5m);
    }

    [Fact]
    public void IcuBeds_DerivesCapacityFromTheThreeStatuses()
    {
        const string json = """
            { "result": {
                "fields": [{ "id": "epi_year" }, { "id": "epi_week" }, { "id": "status" }, { "id": "count" }],
                "records": [
                  { "epi_year": "2023", "epi_week": "2023-09", "status": "COVID", "count": "2.7" },
                  { "epi_year": "2023", "epi_week": "2023-09", "status": "Empty", "count": "109.0" },
                  { "epi_year": "2023", "epi_week": "2023-09", "status": "Non-COVID", "count": "188.6" }
                ], "total": 3 } }
            """;

        var result = new LongFormatParser(DatasetSpecs.IcuBeds).Parse(JsonTable.Load(json));

        result.Single(o => o.MetricCode == MetricCode.IcuBedsCovid.Value).Value.ShouldBe(2.7m);
        result.Single(o => o.MetricCode == MetricCode.IcuBedsTotalCapacity.Value).Value.ShouldBe(300.3m);
    }

    [Fact]
    public void IcuBeds_DoesNotConfuseCovidWithNonCovid()
    {
        const string json = """
            { "result": {
                "fields": [{ "id": "epi_week" }, { "id": "status" }, { "id": "count" }],
                "records": [{ "epi_week": "2023-09", "status": "Non-COVID", "count": "188.6" }],
                "total": 1 } }
            """;

        var result = new LongFormatParser(DatasetSpecs.IcuBeds).Parse(JsonTable.Load(json));

        result.ShouldNotContain(o => o.MetricCode == MetricCode.IcuBedsCovid.Value);
    }

    // ---- Failure modes ----

    [Fact]
    public void MissingField_ThrowsListingWhatWasAvailable()
    {
        const string json = """
            { "result": { "fields": [{ "id": "epi_week" }, { "id": "something_else" }],
              "records": [{ "epi_week": "2023-09", "something_else": "100" }], "total": 1 } }
            """;

        var ex = Should.Throw<InvalidOperationException>(
            () => new LongFormatParser(DatasetSpecs.Infections).Parse(JsonTable.Load(json)));

        ex.Message.ShouldContain("something_else");
    }

    [Fact]
    public void UnmappedStatusValues_ProduceNoObservationsAndThrow()
    {
        const string json = """
            { "result": { "fields": [{ "id": "epi_week" }, { "id": "status" }, { "id": "count" }],
              "records": [{ "epi_week": "2023-09", "status": "Deceased", "count": "3.0" }], "total": 1 } }
            """;

        Should.Throw<InvalidOperationException>(
            () => new LongFormatParser(DatasetSpecs.IcuBeds).Parse(JsonTable.Load(json)));
    }

    // ---- JsonTable ----

    [Fact]
    public void JsonTable_ExcludesTheCkanRowId()
    {
        const string json = """
            { "result": { "fields": [{ "id": "_id" }, { "id": "epi_week" }, { "id": "count" }],
              "records": [{ "_id": 1, "epi_week": "2023-09", "count": "10" }], "total": 1 } }
            """;

        JsonTable.Load(json).Fields.ShouldBe(["epi_week", "count"]);
    }

    [Fact]
    public void JsonTable_HandlesNumericAndNullValues()
    {
        const string json = """
            { "result": { "fields": [{ "id": "epi_week" }, { "id": "est_count" }],
              "records": [
                { "epi_week": "2023-09", "est_count": 58300 },
                { "epi_week": "2023-10", "est_count": null }
              ], "total": 2 } }
            """;

        var result = new LongFormatParser(DatasetSpecs.Infections).Parse(JsonTable.Load(json));

        result.Count.ShouldBe(1);
        result[0].Value.ShouldBe(58300m);
    }

    [Fact]
    public void JsonTable_AssemblesMultiplePages()
    {
        static string Page(string week, string count) => $$"""
            { "result": { "fields": [{ "id": "epi_week" }, { "id": "count" }],
              "records": [{ "epi_week": "{{week}}", "count": "{{count}}" }], "total": 2 } }
            """;

        JsonTable.Load([Page("2023-09", "100"), Page("2023-10", "200")]).Rows.Count.ShouldBe(2);
    }

    [Fact]
    public void JsonTable_RejectsAMalformedResponse()
    {
        Should.Throw<InvalidOperationException>(() => JsonTable.Load("""{ "error": "nope" }"""));
    }
}