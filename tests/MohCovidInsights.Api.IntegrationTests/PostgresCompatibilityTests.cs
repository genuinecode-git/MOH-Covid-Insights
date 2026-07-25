using Microsoft.EntityFrameworkCore;
using MohCovidInsights.Application.Abstractions;
using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;
using MohCovidInsights.Infrastructure.Persistence;
using MohCovidInsights.Infrastructure.Repositories;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace MohCovidInsights.Api.IntegrationTests;

public sealed class PostgresCompatibilityTests : IAsyncLifetime
{
    private static readonly bool DockerAvailable = CheckDocker();

    private static bool CheckDocker()
    {
        try
        {
            using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            process!.WaitForExit(3000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    
    private readonly PostgreSqlContainer? _postgres = DockerAvailable
        ? new PostgreSqlBuilder("postgres:16-alpine").WithDatabase("mohcovid").Build()
        : null;

    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        if (_postgres is null) return;

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _db = new AppDbContext(options);
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_postgres is null) return;
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private static WeeklyObservation Obs(int year, int week, MetricCode metric, decimal value,
        string dimension = "all") => new(new EpiWeek(year, week), metric, value, dimension);

    [SkippableFact]
    public async Task Model_CreatesSchemaOnPostgres()
    {
        Skip.IfNot(DockerAvailable, "Docker is not running.");

        var tables = await _db.Database
            .SqlQuery<string>($"""
                SELECT table_name AS "Value" FROM information_schema.tables
                WHERE table_schema = 'public'
                """)
            .ToListAsync();

        tables.ShouldContain("observations");
        tables.ShouldContain("sync_runs");
    }

    [SkippableFact]
    public async Task CompositeKey_RejectsDuplicateObservations()
    {
        Skip.IfNot(DockerAvailable, "Docker is not running.");

        _db.Observations.Add(Obs(2023, 9, MetricCode.EstimatedInfections, 58_300));
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        _db.Observations.Add(Obs(2023, 9, MetricCode.EstimatedInfections, 99_999));

        await Should.ThrowAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task DecimalPrecision_SurvivesRoundTrip()
    {
        Skip.IfNot(DockerAvailable, "Docker is not running.");

        _db.Observations.Add(Obs(2023, 20, MetricCode.IcuBedsTotalCapacity, 1396.75m));
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var stored = await _db.Observations
            .SingleAsync(o => o.EpiWeekKey == new EpiWeek(2023, 20).ToKey());
        stored.Value.ShouldBe(1396.75m);
    }

    [SkippableFact]
    public async Task RangeQuery_TranslatesAndFilters()
    {
        Skip.IfNot(DockerAvailable, "Docker is not running.");

        _db.Observations.AddRange(
            Obs(2023, 1, MetricCode.EstimatedInfections, 100),
            Obs(2023, 26, MetricCode.EstimatedInfections, 200),
            Obs(2024, 1, MetricCode.EstimatedInfections, 400));
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var repository = new ObservationRepository(_db);
        var results = await repository.GetAsync(
            new EpiWeekRange(new EpiWeek(2023, 26), new EpiWeek(2024, 1)),
            [MetricCode.EstimatedInfections], "all");

        results.Count.ShouldBe(2);
    }

    [SkippableFact]
    public async Task GetAllDimensions_ReturnsEveryAgeGroup()
    {
        Skip.IfNot(DockerAvailable, "Docker is not running.");

        _db.Observations.AddRange(
            Obs(2023, 9, MetricCode.AvgHospitalisedCases, 1.7m, "0-11"),
            Obs(2023, 9, MetricCode.AvgHospitalisedCases, 34.9m, "60+"),
            Obs(2023, 9, MetricCode.AvgHospitalisedCases, 36.6m, "all"));
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var repository = new ObservationRepository(_db);
        var results = await repository.GetAllDimensionsAsync(
            new EpiWeekRange(new EpiWeek(2023, 9), new EpiWeek(2023, 9)),
            [MetricCode.AvgHospitalisedCases]);

        results.Select(o => o.Dimension).Distinct().Count().ShouldBe(3);
    }

    [SkippableFact]
    public async Task SyncRunTimestamps_AggregateOnPostgres()
    {
        Skip.IfNot(DockerAvailable, "Docker is not running.");

        var run = DatasetSyncRun.Start("d_test", DateTimeOffset.UtcNow);
        run.Succeed(52, "hash", DateTimeOffset.UtcNow);
        _db.SyncRuns.Add(run);
        await _db.SaveChangesAsync();

        var latest = await _db.SyncRuns
            .OrderByDescending(r => r.StartedAt)
            .Select(r => r.PayloadHash)
            .FirstOrDefaultAsync();

        latest.ShouldBe("hash");
    }
}