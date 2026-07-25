using MohCovidInsights.Domain.Entities;
using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Infrastructure.Persistence;

public sealed class DevDataSeeder(AppDbContext db, ILogger<DevDataSeeder> logger)
{
    private const int IcuLagWeeks = 2;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Observations.AnyAsync(ct))
        {
            logger.LogInformation("Observations already present, skipping seed");
            return;
        }

        var range = new EpiWeekRange(new EpiWeek(2023, 1), new EpiWeek(2024, 4));
        var weeks = range.Enumerate().ToList();
        var observations = new List<WeeklyObservation>();

        var infections = new decimal[weeks.Count];
        for (var i = 0; i < weeks.Count; i++)
        {
            var wave = Math.Sin(i / 4.2) * 0.5 + Math.Sin(i / 11.0) * 0.35 + 1.0;
            infections[i] = Math.Round((decimal)(38_000 + wave * 24_000), 0);
        }

        for (var i = 0; i < weeks.Count; i++)
        {
            var week = weeks[i];
            var driver = infections[Math.Max(0, i - IcuLagWeeks)];

            var hospitalAdmissions = Math.Round(driver * 0.0032m + 90m, 0);
            var icuAdmissions = Math.Round(hospitalAdmissions * 0.13m + 4m, 0);
            var capacity = 1396m;
            var covidBeds = Math.Round(icuAdmissions * 9.8m, 0);
            var nonCovidBeds = Math.Round(capacity * 0.505m, 0);

            observations.Add(new WeeklyObservation(week, MetricCode.EstimatedInfections, infections[i]));
            observations.Add(new WeeklyObservation(week, MetricCode.HospitalAdmissions, hospitalAdmissions));
            observations.Add(new WeeklyObservation(week, MetricCode.IcuAdmissions, icuAdmissions));
            observations.Add(new WeeklyObservation(week, MetricCode.AvgHospitalisedCases, Math.Round(hospitalAdmissions * 3.2m, 0)));
            observations.Add(new WeeklyObservation(week, MetricCode.AvgIcuCases, Math.Round(icuAdmissions * 4.1m, 0)));
            observations.Add(new WeeklyObservation(week, MetricCode.IcuBedsCovid, covidBeds));
            observations.Add(new WeeklyObservation(week, MetricCode.IcuBedsNonCovid, nonCovidBeds));
            observations.Add(new WeeklyObservation(week, MetricCode.IcuBedsTotalCapacity, capacity));
        }

        db.Observations.AddRange(observations);

        var now = new DateTimeOffset(2024, 6, 6, 0, 0, 0, TimeSpan.Zero);
        foreach (var datasetId in new[] { "d_seed_infections", "d_seed_admissions", "d_seed_average_cases", "d_seed_icu_beds" })
        {
            var run = DatasetSyncRun.Start(datasetId, now);
            run.Succeed(weeks.Count, "seed", now);
            db.SyncRuns.Add(run);
        }

        await db.SaveChangesAsync(ct);
        logger.LogWarning("Seeded {Count} synthetic observations across {Weeks} epi weeks — DEVELOPMENT DATA",
            observations.Count, weeks.Count);
    }
}