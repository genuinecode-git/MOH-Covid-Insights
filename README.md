# MohCovidInsights

A COVID insights analytics solution built with:
- **ASP.NET Core / Lambda container** for the backend API and ingest services
- **React 19 + Vite** for the web SPA
- **AWS-native architecture** with CloudFront, S3, API Gateway, Lambda, Aurora Serverless, Secrets Manager, EventBridge, and VPC networking

## Repository Structure
- `src/MohCovidInsights.Api/` — ASP.NET API project
- `src/MohCovidInsights.Application/` — application services, business logic, and validation
- `src/MohCovidInsights.Infrastructure/` — data access, persistence, AWS integration, and parsing
- `src/MohCovidInsights.Ingestion/` — scheduled ingest/sync Lambda project
- `src/MohCovidInsights.Web/` — React frontend
- `docs/` — architecture, deployment, and package documentation

## Useful Documentation
- [Developer Guide](docs/developer-guide.md) — A-Z for developers how to setup the application on local
- [AWS Setup](docs/aws-setup.md) — architecture overview and AWS service topology
- [Packages Used](docs/packages-used.md) — list of frontend and backend dependencies
- [GitHub Workflows](docs/github-workflows.md) — CI, Terraform validation, and deployment workflows
- [GitHub OIDC Setup](docs/github-oidc-setup.md) — GitHub Actions OIDC setup guidance

## Getting Started
1. Restore and build the solution:
   ```bash
   dotnet restore
   dotnet build
   ```
2. Install frontend dependencies:
   ```bash
   cd src/MohCovidInsights.Web
   npm install
   npm run build
   ```

## Notes
- The backend targets **.NET 9**.
- The frontend uses **TypeScript**, **MUI**, **React Query**, and **Recharts**.

## Sucessful page

### Dashboard
<img width="760" height="473" alt="image" src="https://github.com/user-attachments/assets/dbcd9938-9d62-4305-97fe-a5e7f09d5c97" />

<img width="1503" height="944" alt="image" src="https://github.com/user-attachments/assets/4b31abd9-0b05-45cd-9f44-8b55268fa15c" />

### Data List
<img width="757" height="473" alt="image" src="https://github.com/user-attachments/assets/494bae31-dbc2-4a82-9996-30533e0590c8" />
