using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MohCovidInsights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "observations",
                columns: table => new
                {
                    epi_week_key = table.Column<int>(type: "INTEGER", nullable: false),
                    metric_code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    dimension = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    value = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observations", x => new { x.epi_week_key, x.metric_code, x.dimension });
                });

            migrationBuilder.CreateTable(
                name: "sync_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    dataset_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    row_count = table.Column<int>(type: "INTEGER", nullable: false),
                    payload_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    failure_reason = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_runs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_observations_metric_week",
                table: "observations",
                columns: new[] { "metric_code", "epi_week_key" });

            migrationBuilder.CreateIndex(
                name: "ix_sync_runs_dataset_started",
                table: "sync_runs",
                columns: new[] { "dataset_id", "started_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "observations");

            migrationBuilder.DropTable(
                name: "sync_runs");
        }
    }
}
