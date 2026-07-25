# GitHub Workflows

This repository uses GitHub Actions workflows to validate code, manage infrastructure, and perform deployments in a controlled way.

## Workflow files
- `.github/workflows/ci.yml`
- `.github/workflows/terraform.yml`
- `.github/workflows/deploy.yml`

## CI (`ci.yml`)

### What it does
- Runs on `push` to `main`, `pull_request`, and manual dispatch.
- Builds and tests the backend solution using .NET.
- Installs frontend dependencies, checks TypeScript, runs linting, tests, and builds the React SPA.
- Builds container images for the API and ingestion services with Docker Buildx.
- Uploads test results and coverage artifacts.

### Why it exists
- Ensures backend and frontend changes are validated before they are merged.
- Confirms the repository builds cleanly on GitHub-hosted runners.
- Provides early detection of regression and compilation issues.
- Verifies container images can be built for Lambda deployment packaging.

## Terraform validation and planning (`terraform.yml`)

### What it does
- Runs on `push` to `main`, `pull_request`, and manual dispatch.
- Limits execution to changes under `infra/**` and `.github/workflows/terraform.yml`.
- Checks Terraform formatting and validates configuration.
- Runs `terraform test` and `tflint` for linting.
- When AWS credentials are available, creates a plan and posts a PR comment with the result.

### Why it exists
- Keeps infrastructure as code safe and reviewable.
- Detects Terraform syntax or linting issues early.
- Gives reviewers a generated plan summary for proposed infrastructure changes.
- Helps avoid accidental deployment mistakes by enforcing validation before merge.

## Deployment (`deploy.yml`)

### What it does
- Runs only via manual `workflow_dispatch`.
- Accepts a target environment: `dev`, `staging`, or `prod`.
- Requires explicit confirmation text (`deploy`) to proceed.
- Builds the frontend SPA.
- Uses Terraform to apply infrastructure changes for the selected environment.
- Outputs deployed site and API URLs to the workflow summary.
- Triggers the ingest Lambda once deployment completes.

### Why it exists
- Keeps deployment intentional and auditable.
- Prevents accidental production deployments by requiring environment selection and confirmation.
- Uses AWS role assumption to securely configure credentials.
- Ensures deployed infrastructure and frontend assets are aligned.

## Notes
- `ci.yml` focuses on code quality, compile/test validation, and artifact preparation.
- `terraform.yml` focuses on infrastructure validation before changes reach the main branch.
- `deploy.yml` is the controlled release mechanism for pushing infrastructure and app updates to AWS.
