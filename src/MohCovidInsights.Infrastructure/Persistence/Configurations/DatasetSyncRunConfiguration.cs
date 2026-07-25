using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MohCovidInsights.Domain.Entities;

namespace MohCovidInsights.Infrastructure.Persistence.Configurations;

public sealed class DatasetSyncRunConfiguration : IEntityTypeConfiguration<DatasetSyncRun>
{
    public void Configure(EntityTypeBuilder<DatasetSyncRun> builder)
    {
        builder.ToTable("sync_runs");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.DatasetId).HasColumnName("dataset_id").HasMaxLength(64).IsRequired();
        builder.Property(r => r.StartedAt).HasColumnName("started_at");
        builder.Property(r => r.CompletedAt).HasColumnName("completed_at");
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.RowCount).HasColumnName("row_count");
        builder.Property(r => r.PayloadHash).HasColumnName("payload_hash").HasMaxLength(64);
        builder.Property(r => r.FailureReason).HasColumnName("failure_reason").HasMaxLength(1024);

        builder.HasIndex(r => new { r.DatasetId, r.StartedAt }).HasDatabaseName("ix_sync_runs_dataset_started");
    }
}