# Packages Used

This document lists the main packages used by the Web frontend and the backend services in the MohCovidInsights solution.

## Web Frontend Packages

### Dependencies
- `@emotion/react` / `@emotion/styled` — styling engine for MUI and CSS-in-JS.
- `@mui/material` — Material UI component library.
- `@mui/icons-material` — MUI icon set.
- `@mui/x-data-grid` — advanced data grid component.
- `@mui/x-date-pickers` — date picker components.
- `@tanstack/react-query` — data fetching, caching, and synchronization.
- `@tanstack/react-query-devtools` — development tooling for React Query.
- `react` / `react-dom` — core React library.
- `react-router-dom` — client-side routing.
- `recharts` — charting library.

### Dev Dependencies
- `vite` — frontend build tool and development server.
- `typescript` — typed JavaScript language.
- `@vitejs/plugin-react` — React support for Vite.
- `eslint`, `@eslint/js`, `eslint-plugin-react-hooks`, `eslint-plugin-react-refresh` — linting.
- `vitest`, `@vitest/coverage-v8` — unit testing and coverage.
- `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event` — React component testing.
- `playwright` / `@playwright/test` — browser automation and end-to-end testing.
- `msw` — mocking service worker for API tests.
- `jsdom` — DOM emulation for tests.
- `@types/node`, `@types/react`, `@types/react-dom` — TypeScript type definitions.

## Backend Packages

### API Project (`src/MohCovidInsights.Api`)
- `Amazon.Lambda.AspNetCoreServer.Hosting` — run ASP.NET Core inside AWS Lambda.
- `Asp.Versioning.Mvc` / `Asp.Versioning.Mvc.ApiExplorer` — API versioning support.
- `AspNetCore.HealthChecks.NpgSql` — Postgres health checks.
- `Microsoft.AspNetCore.OpenApi` — OpenAPI / Swagger support.
- `Microsoft.EntityFrameworkCore.Design` — EF Core design-time tools.
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` — EF Core health checks.
- `Scalar.AspNetCore` — enhanced logging helpers for structured logs.
- `Serilog.AspNetCore` — structured logging integration.

### Infrastructure Project (`src/MohCovidInsights.Infrastructure`)
- `AWSSDK.SecretsManager` — AWS Secrets Manager integration.
- `Microsoft.EntityFrameworkCore` — ORM.
- `Microsoft.EntityFrameworkCore.Design` — EF Core design-time tools.
- `Microsoft.EntityFrameworkCore.Sqlite` — SQLite provider for local development.
- `Microsoft.Extensions.Caching.Memory` — in-memory caching.
- `Microsoft.Extensions.Http.Resilience` — resilient HTTP client support.
- `Microsoft.Extensions.Options.DataAnnotations` — options validation.
- `Npgsql.EntityFrameworkCore.PostgreSQL` — Postgres database provider.
- `Scrutor` — assembly scanning and DI registration helpers.

### Application Project (`src/MohCovidInsights.Application`)
- `FluentValidation` — request and domain validation.
- `Microsoft.Extensions.DependencyInjection.Abstractions` — DI abstractions.
- `Microsoft.Extensions.Logging.Abstractions` — logging abstractions.

## Notes
- The backend uses `.NET 9.0` across all projects.
- The Web frontend uses `React 19`, `Vite`, and TypeScript.
