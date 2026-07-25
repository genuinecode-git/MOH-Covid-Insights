using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MohCovidInsights.Domain.Entities;

namespace MohCovidInsights.Infrastructure.Persistence.Configurations;

public sealed class WeeklyObservationConfiguration : IEntityTypeConfiguration<WeeklyObservation>
{
    public void Configure(EntityTypeBuilder<WeeklyObservation> builder)
    {
        builder.ToTable("observations");

        builder.HasKey(o => new { o.EpiWeekKey, o.MetricCode, o.Dimension });

        builder.Property(o => o.EpiWeekKey).HasColumnName("epi_week_key");
        builder.Property(o => o.MetricCode).HasColumnName("metric_code").HasMaxLength(64);
        builder.Property(o => o.Dimension).HasColumnName("dimension").HasMaxLength(64);
        builder.Property(o => o.Value).HasColumnName("value").HasPrecision(18, 2);

        builder.Ignore(o => o.EpiWeek);
        builder.Ignore(o => o.Metric);

        builder.HasIndex(o => new { o.MetricCode, o.EpiWeekKey }).HasDatabaseName("ix_observations_metric_week");
    }
}