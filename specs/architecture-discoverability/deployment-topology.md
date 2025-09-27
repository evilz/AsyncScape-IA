# Deployment Topology: Architecture Discoverability Hub

## Service Composition (Aspire)
- `AsyncScapeIA.WebApp`: ASP.NET Core Blazor + Minimal APIs, exposed via HTTPS (5001). Configured as a service in Aspire AppHost.
- `AsyncScapeIA.Workers`: BackgroundService host processing scheduled sync jobs and audit retention. Runs as Aspire project with dependency on PostgreSQL and storage.
- `postgres`: PostgreSQL 16 container managed by Aspire; data persisted via local volume or bound to external database when deployed outside Aspire.
- `storage`: MinIO-compatible object storage container (or connection to external S3-compatible service) managed through Aspire resource definitions.
- `identity`: Development OpenID Connect provider (Keycloak/Entra simulator) orchestrated by Aspire for local testing; replace with environment-specific IdP in production.
- `otel`: OpenTelemetry collector resource defined in Aspire to aggregate traces, metrics, and logs.

## Network Flow
```
[Browser]
   |
[Reverse Proxy (optional in prod)]
   |---> WebApp (HTTPS)
   |        |--> PostgreSQL (5432)
   |        |--> Storage (9000)
   |        |--> Identity (HTTPS)
   |        \--> OTEL Collector (4317)
   |
   \---> Workers (shared services)
```

## Environment Strategy
- **Development/Test**: Use `.NET Aspire AppHost` (`dotnet run --project Aspire.AppHost`) to start all services, including telemetry and identity simulator. Aspire dashboard provides health, logs, metrics, and trace inspection.
- **Production**: Deploy WebApp/Workers containers to target infrastructure (Kubernetes, VMs). Use same configuration model but substitute managed PostgreSQL, S3-compatible storage, and corporate IdP. Aspire is optional but can act as deployment template.

## Configuration & Secrets
- Managed via AppHost environment variables or secret providers (user-secrets, environment). For production, integrate HashiCorp Vault or platform-specific secret store.
- Keys include `ConnectionStrings__Catalog`, `Storage__Endpoint`, `Auth__Authority`, `Auth__ClientId`, `Telemetry__OtlpEndpoint`.

## Scaling & Resilience
- WebApp scales horizontally; session state remains stateless. Data protection keys stored in storage provider configured via Aspire resource (e.g., Redis or storage bucket) if sticky sessions avoided.
- Workers scale by instantiating additional service replicas in Aspire (development) or container orchestrator (production).
- PostgreSQL backup/restore handled via scheduled jobs; Aspire provides local volume but production relies on managed database backups.

## Monitoring & SLA Alignment
- Aspire exposes OpenTelemetry pipelines; forward metrics to Prometheus or Grafana Cloud equivalent. Alert on:
  - Availability <99.5%
  - Sync lag >15 minutes
  - API latency >2s p95
  - Worker failure rate >2% over 10 minutes
- Aspire dashboard acts as first-line observability tool during development.

## Disaster Recovery
- RTO target 30 minutes: redeploy WebApp/Workers containers and restore database snapshot.
- RPO target 15 minutes: rely on frequent connector syncs + database point-in-time restore.
- Storage replication recommended for object assets (MinIO bucket replication in production).

## Simplicity Guidance
- Keep infrastructure minimal: WebApp + Workers + Postgres + Storage + IdP + OTEL.
- Defer optional components (message brokers, advanced caching) until validated by load tests.
