# MOH COVID Insights

A dashboard over Singapore Ministry of Health COVID-19 datasets, ingesting four
weekly (epi-week) series from data.gov.sg and presenting them as an interactive
dashboard plus a per-dataset table view.

- **Backend** — ASP.NET Core (.NET 9), Clean Architecture, CQRS
- **Frontend** — React 19 + TypeScript + Material UI, Vite
- **Data** — data.gov.sg CKAN API, ingested into SQLite (local) or Postgres (deployed)
- **Infrastructure** — Terraform (AWS Lambda, API Gateway, Aurora Serverless, CloudFront)

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 9.0.x | Pinned in `global.json` |
| Node.js | 20+ | For the frontend |
| Docker | any recent | Optional — only for the Postgres integration tests |

Verify:

    dotnet --version
    node -v

## Quick start

From a clone, this brings up the API with real MOH data and the dashboard in
about a minute.

### 1. Run the API

    dotnet run --project src/MohCovidInsights.Api

The API starts on `http://localhost:5191`. On first run in Development it creates
a local SQLite database (`mohcovid.db`) and applies migrations automatically.

Interactive API docs are at `http://localhost:5191/scalar/v1`.

### 2. Load the data

The database starts empty, so trigger an ingestion. In a second terminal:

    curl -X POST http://localhost:5191/api/v1/admin/sync

This pulls all four datasets from data.gov.sg. It takes ~25 seconds — there is a
short pause between datasets to respect the upstream rate limit (roughly five
requests per minute). A successful run reports:

    { "succeeded": 4, "skipped": 0, "failed": 0, "totalRows": ... }

Re-running is safe: unchanged datasets are skipped by a payload hash, so a second
sync reports `"skipped": 4` and writes nothing.

### 3. Run the frontend

    cd src/MohCovidInsights.Web
    npm install
    npm run dev

Open `http://localhost:5173`. The Vite dev server proxies `/api` to the backend,
so no CORS configuration is needed locally.

## Verifying it works

Query a week with known published figures — epi week 50 of 2023:

    curl "http://localhost:5191/api/v1/dashboard?from=2023-W50&to=2023-W50"

The response should show 58,300 estimated infections, 965 hospital admissions,
and 32 ICU admissions. These are MOH's published values, so a match confirms the
full pipeline from upstream API to dashboard JSON.

## Project layout

    src/
      MohCovidInsights.Domain/          Entities, value objects (EpiWeek, MetricCode). No dependencies.
      MohCovidInsights.Application/     CQRS handlers, DTOs, ports, analytics.
      MohCovidInsights.Infrastructure/  EF Core, data.gov.sg client, dataset parsers.
      MohCovidInsights.Api/             Controllers, DI wiring, Program.cs.
      MohCovidInsights.Ingestion/       Scheduled-sync entry point (runs as a Lambda when deployed).
      MohCovidInsights.Web/             React + Vite frontend.
    tests/
      *.Domain.Tests, *.Application.Tests, *.Infrastructure.Tests
      *.Api.IntegrationTests            End-to-end and Postgres-compatibility suites.
      *.Architecture.Tests              Enforces the dependency direction between layers.
    infra/                              Terraform for AWS deployment.

## Running the tests

All backend tests:

    dotnet test

The Postgres compatibility suite needs Docker. Without it, those tests skip
cleanly and the rest run:

    dotnet test --filter "FullyQualifiedName!~PostgresCompatibility"

Frontend tests:

    cd src/MohCovidInsights.Web
    npm test

## Running against Postgres locally

SQLite is the default and needs nothing. To run against Postgres — matching the
deployed target — bring one up with Docker Compose:

    docker compose up -d db

Then set the provider before running the API:

    Database__Provider=Postgres \
    ConnectionStrings__Default="Host=localhost;Port=5432;Database=mohcovid;Username=dbadmin;Password=localdev" \
    dotnet run --project src/MohCovidInsights.Api

## The datasets

Four epi-week series from data.gov.sg collection 522, all published in tidy
(long) format:

| Dataset | data.gov.sg ID |
|---------|----------------|
| COVID-19 infections by epi-week | `d_11e68bba3b3c76733475a72d09759eeb` |
| New hospitalisation / ICU admissions by epi-week | `d_98e8d8ba612a748413c439550c3c6942` |
| Average daily hospitalised / ICU cases by epi-week | `d_0d1da54a73733d33e40f662f757af537` |
| Average daily adult ICU bed utilisation by epi-week | `d_ac42b0ea4ae0528bc9dbef90f0658f2b` |

Dataset IDs and their parser configuration live in `appsettings.json` under
`DataGovSg:Datasets`.

## Note on the data

These are archived datasets covering roughly January 2023 to January 2024, last
updated by MOH in mid-2024. The dashboard presents historical figures — it is not
a live health-monitoring service.

## Troubleshooting

**Sync reports a failure.** The response lists which dataset and why. A missing
field error names the fields the upstream response actually contained, which
usually means a dataset's schema differs from its parser configuration in
`appsettings.json`.

**429 Too Many Requests during sync.** The upstream rate limit was hit. Wait a
minute and retry; the sync paces itself, but a rapid re-run can still trip it.

**Dashboard returns 404 "No data available".** The database is empty — run the
sync (step 2).

**Frontend shows a connection error.** Confirm the API is running on port 5191;
the Vite proxy target is set in `vite.config.ts`.