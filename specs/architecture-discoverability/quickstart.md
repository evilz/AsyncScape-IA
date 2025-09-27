# Quickstart: Architecture Discoverability Hub

## Purpose
This guide walks contributors through running the Architecture Discoverability Hub locally using .NET Aspire for orchestration, executing smoke tests, and understanding the primary user flows.

## Prerequisites
- .NET SDK 10 preview (ensure global.json alignment when added)
- Docker or compatible container runtime (required by Aspire)
- Node.js 20+ (Playwright dependencies)
- Make or PowerShell for automation scripts

## 1. Clone & Bootstrap
```
git clone <repo-url>
cd AsyncScape-IA
```
Install .NET workloads:
```
dotnet workload update
```

## 2. Start Local Dependencies with Aspire
Launch the Aspire AppHost to spin up PostgreSQL, MinIO-compatible storage, a development OpenID Connect provider, and the OpenTelemetry collector:
```
dotnet run --project Aspire.AppHost
```
Aspire dashboard (default `https://localhost:12021`) shows service status, logs, and telemetry pipelines.

## 3. Configure Environment
Copy the sample environment file:
```
cp src/AsyncScapeIA.WebApp/appsettings.Development.json.sample src/AsyncScapeIA.WebApp/appsettings.Development.json
```
Key settings to adjust:
- `ConnectionStrings:Catalog`
- `Storage:Minio` endpoint, access key, secret
- `Auth:Authority`, `Auth:ClientId`, `Auth:ClientSecret`
- `Telemetry:OtlpEndpoint` (Aspire collector by default)

## 4. Apply Database Migrations & Seed Data
Run the WebApp project to apply EF Core migrations and seed mock data (components, ownership groups, sample connector):
```
dotnet run --project src/AsyncScapeIA.WebApp
```

## 5. Launch Background Worker
Start the worker project that handles scheduled sync jobs using native BackgroundService scheduling:
```
dotnet run --project src/AsyncScapeIA.Workers
```
Workers share the Aspire-managed infrastructure and execute every five minutes by default.

## 6. Access the Portal
Browse to `https://localhost:5001` (self-signed cert). Sign in with seeded test users from the Aspire identity provider (e.g., `admin@example.com`, `curator@example.com`).

Explore the primary UI areas:
- **Catalog Overview**: searchable architecture component list
- **Schema Library**: rendered OpenAPI/AsyncAPI documentation
- **Connector Console**: configure connectors, inspect recent syncs
- **Ownership Dashboard**: manage owner groups and escalation paths

## 7. Trigger Manual Sync
From the Connector Console, run a manual sync for the sample connector and confirm:
- Sync job recorded with status Succeeded
- Schema Library displays updated artifact
- Audit entry logged with correlation id

## 8. Run Test Suites
Install Playwright browsers:
```
dotnet tool restore
pwsh ./build/install-playwright.ps1
```
Run the automated tests (will fail until implementation complete):
```
dotnet test --filter Category=Unit
 dotnet test --filter Category=Integration
 dotnet test --filter Category=Component
 dotnet test --filter Category=E2E
```
Optional performance smoke test:
```
dotnet test --filter Category=Performance
```

## 9. Observability Checks
- Use the Aspire dashboard to inspect resource health, logs, and OpenTelemetry traces.
- If exporting to external backends, connect Prometheus/Grafana or Tempo/Jaeger using the OTEL collector endpoints defined in AppHost.
- Verify `/health/ready` responds with overall service health.

## 10. Shutdown & Cleanup
Stop Aspire-managed services:
```
dotnet run --project Aspire.AppHost -- --stop
```
Drop or reset the database if a clean slate is needed before the next run.

## Next Steps
- Implement connector providers in `AsyncScapeIA.Integrations` following the contract specification.
- Customize UI themes via Simple/UI tokens.
- Update documentation as workflows evolve or new components are introduced.
