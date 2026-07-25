using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MohCovidInsights.Application.Dashboard.Dtos;
using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Infrastructure.Persistence;
using MohCovidInsights.Infrastructure.Sync;
using MohCovidInsights.Infrastructure.Sync.Interfaces;

using Shouldly;
using WireMock.Server;

namespace MohCovidInsights.Api.IntegrationTests;

public sealed class IngestionPipelineTests : IAsyncLifetime
{
    private WireMockServer _upstream = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private readonly string _dbName = $"pipeline-{Guid.NewGuid()}.db";

    public Task InitializeAsync()
    {
        _upstream = WireMockServer.Start();

        CkanStub.Register(_upstream, "d_infections", CkanStub.Body(
            ["epi_year", "epi_week", "est_count"],
            [["2023", "2023-49", "56043"], ["2023", "2023-50", "58300"]]));

        CkanStub.Register(_upstream, "d_admissions", CkanStub.Body(
            ["epi_year", "epi_week", "new_admisison_type", "count"],
            [["2023", "2023-49", "Hospitalised", "763"],
             ["2023", "2023-49", "ICU", "23"],
             ["2023", "2023-50", "Hospitalised", "965"],
             ["2023", "2023-50", "ICU", "32"]]));

        CkanStub.Register(_upstream, "d_icu_beds", CkanStub.Body(
            ["epi_year", "epi_week", "status", "count"],
            [["2023", "2023-49", "COVID", "300"],
             ["2023", "2023-49", "Non-COVID", "700"],
             ["2023", "2023-49", "Empty", "396"],
             ["2023", "2023-50", "COVID", "312"],
             ["2023", "2023-50", "Non-COVID", "707"],
             ["2023", "2023-50", "Empty", "377"]]));

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = $"Data Source={_dbName}",
                    ["Database:Provider"] = "Sqlite",
                    ["DataGovSg:BaseUrl"] = _upstream.Url,
                    ["DataGovSg:DatasetDelay"] = "00:00:00",
                    ["DataGovSg:Datasets:0:Id"] = "d_infections",
                    ["DataGovSg:Datasets:0:Name"] = "Infections",
                    ["DataGovSg:Datasets:0:Parser"] = "infections",
                    ["DataGovSg:Datasets:0:Tone"] = "infections",
                    ["DataGovSg:Datasets:1:Id"] = "d_admissions",
                    ["DataGovSg:Datasets:1:Name"] = "Admissions",
                    ["DataGovSg:Datasets:1:Parser"] = "admissions",
                    ["DataGovSg:Datasets:1:Tone"] = "hospital",
                    ["DataGovSg:Datasets:2:Id"] = "d_icu_beds",
                    ["DataGovSg:Datasets:2:Name"] = "ICU Beds",
                    ["DataGovSg:Datasets:2:Parser"] = "icu-beds",
                    ["DataGovSg:Datasets:2:Tone"] = "utilisation",
                });
            });   
        });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        _upstream.Stop();
        if (File.Exists(_dbName)) File.Delete(_dbName);
    }

    private async Task<SyncSummary> IngestAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var summary = await scope.ServiceProvider
            .GetRequiredService<IDatasetSyncService>().SyncAllAsync();

        if (summary.Failed > 0)
        {
            var reasons = await db.SyncRuns
                .Where(r => r.Status == SyncStatus.Failed)
                .Select(r => $"{r.DatasetId}: {r.FailureReason}")
                .ToListAsync();

            throw new InvalidOperationException(
                $"Sync had {summary.Failed} failure(s): {string.Join(" | ", reasons)}");
        }

        return summary;
    }
    
    [Fact]
    public async Task UpstreamJson_BecomesDashboardValues()
    {
        var summary = await IngestAsync();
        summary.Failed.ShouldBe(0);

        // prove rows actually landed
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.Observations
                .Select(o => new { o.MetricCode, o.Dimension })
                .Distinct()
                .ToListAsync();

            stored.ShouldNotBeEmpty("sync reported success but wrote no observations");
        }

        var response = await _factory.CreateClient()
            .GetAsync("/api/v1/dashboard?from=2023-W49&to=2023-W50");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());

        var dto = await response.Content.ReadFromJsonAsync<DashboardDto>();
        var week50 = dto!.Weekly.Single(w => w.EpiWeek == "2023-50");
        week50.EstimatedInfections.ShouldBe(58_300m);
    }

    [Fact]
    public async Task Infections_UsesTheEstCountField()
    {
        await IngestAsync();

        var dto = await _factory.CreateClient()
            .GetFromJsonAsync<DashboardDto>("/api/v1/dashboard?from=2023-W49&to=2023-W50");

        dto!.Kpis.Single(k => k.Id == "total-infections").Value.ShouldNotBe("0");
    }

    [Fact]
    public async Task BedCapacity_IsDerivedFromTheThreeStatuses()
    {
        await IngestAsync();

        var dto = await _factory.CreateClient()
            .GetFromJsonAsync<DashboardDto>("/api/v1/dashboard?from=2023-W49&to=2023-W50");

        dto!.BedUtilisation.TotalCapacity.ShouldBe(1396m);
        dto.BedUtilisation.Segments.Single(s => s.Label == "Empty Beds").Beds.ShouldBe(377m);
    }

    [Fact]
    public async Task ReIngestion_SkipsUnchangedDatasets()
    {
        await IngestAsync();
        var second = await IngestAsync();

        second.Skipped.ShouldBe(3);
        second.Succeeded.ShouldBe(0);
    }
    
    [Fact]
    public async Task InvalidEpiWeek_ReturnsProblemDetails()
    {
        await IngestAsync();

        var response = await _factory.CreateClient()
            .GetAsync("/api/v1/dashboard?from=garbage&to=2023-W50");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("garbage");
    }
}