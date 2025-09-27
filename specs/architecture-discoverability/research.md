# Research: Architecture Discoverability Hub (Phase 0)

## Scope & Goals
- Ensure all planned tooling and components are free or open source, including SysinfoFocus Simple/UI for Blazor.
- Identify best practices for ASP.NET Core (.NET 10) with Blazor UI, minimal APIs, and native scheduling primitives without third-party job managers.
- Define integration strategy for OpenAPI/AsyncAPI-driven connectors (Confluent Schema Registry, GitHub, Azure API Management, internal artifact stores) while remaining hosting-provider agnostic.
- Establish storage, security, observability, and testing strategies that work in self-managed, on-prem, or cloud environments.

## UI & Component Libraries
- **Blazor Strategy**: Utilize Blazor Server with .NET 10 enhancements (streaming rendering, per-component lazy loading) for responsive, real-time UI updates.
- **Component Framework**: SysinfoFocus Simple/UI is explicitly free to use (“FREE TO USE UI component library for Blazor”) and MIT-licensed. Provides responsive layouts and Tailwind-friendly theming; supports mobile-first design.
- **Accessibility & Theming**: Adopt CSS variable-driven theming and integrate automatic accessibility checks (axe-core via Playwright). Provide light/dark themes tailored to Simple/UI tokens.
- **Fallback Option**: Maintain compatibility with MudBlazor (MIT) should Simple/UI lag behind .NET 10 releases; abstract shared UI primitives to ease swap.

## Minimal API & Hosting
- **Minimal API Usage**: Serve export endpoints and webhook callbacks through Minimal APIs with `.MapGroup`, endpoint filters, and `.WithOpenApi()` metadata. Supports versioned groups and policy-based authorization.
- **Hosting Model**: Distribute as containerized workloads (Docker/OCI) deployable to Kubernetes, Nomad, Azure Kubernetes Service, AWS ECS, or bare-metal clusters. No runtime dependency on a specific cloud provider.
- **Configuration Management**: Use .NET configuration providers layered with environment variables, JSON, and optional secrets from HashiCorp Vault or Doppler (both offer free/open tooling). Avoid provider-specific services.
- **Aspire in Development**: Use .NET Aspire AppHost + ServiceDefaults for local composition, but keep production deployment scripts independent (e.g., Helm charts, docker-compose).

## Background Orchestration (Native .NET 10)
- Implement scheduling with `BackgroundService`, `PeriodicTimer`, and `Task.Delay` for cadence control, avoiding external schedulers.
- Employ `Channel` for job queue buffering and `IHostedLifecycleService` to coordinate startup/shutdown.
- Integrate Polly’s `ResiliencePipeline` for retries, circuit breaking, and fallback handling using open-source resilience primitives.

## Data Storage & Seed Strategy
- **Operational Data**: PostgreSQL 16 (self-hosted or managed) accessed via Npgsql 8.x and EF Core 10. Execute migrations at startup with `IMigrator`. Seed mock data the first time the database is empty.
- **Specification Assets**: Store OpenAPI/AsyncAPI files in S3-compatible object storage (e.g., MinIO, Ceph, or cloud buckets) with encryption-at-rest and lifecycle rules (archive/cleanup after 180 days of inactivity).
- **Audit Trail**: Partition audit tables monthly and retain 24 months of history. Run cleanup BackgroundService to enforce retention while logging deletions.

## Connectors & External Integrations
- Focus connectors on systems that publish OpenAPI/AsyncAPI artifacts:
  - Confluent Schema Registry (Apache 2.0 client libraries).
  - GitHub-hosted specifications via Octokit.NET (MIT) with PAT or GitHub App authentication.
  - Azure API Management or Event Grid schemas via Azure SDK for .NET (MIT) when available.
  - Internal artifact repositories exposed through HTTP/JSON endpoints or file shares.
- Standardize connector contract to fetch specs, metadata, and ownership hints; rely on REST/gRPC interfaces when possible to remain platform neutral.

## Authentication & Authorization
- Employ OpenID Connect-compliant identity provider (Azure AD, Keycloak, Okta, Auth0) via Microsoft.Identity.Web or generic `AddOpenIdConnect` middleware.
- Define application roles (PlatformAdmin, ArchitectureCurator, Contributor, Viewer) within the IdP. Map to ASP.NET Core authorization policies.
- Support both delegated user flows and confidential client credentials for automation. Embrace PKCE and MFA through IdP policies.

## Observability & Telemetry
- Standardize on OpenTelemetry for traces, metrics, and logs. Export via OTLP to open-source backends (Prometheus + Grafana, Jaeger, Loki).
- Instrument connector lifecycle stages (discovery, fetch, transform, publish), sync latency, and override activities.
- Provide readiness/liveness/startup endpoints using `MapHealthChecks` with checks for PostgreSQL, object storage, and connector health probes.
- Define alert thresholds: sync lag >15 minutes, API latency >2 seconds p95, job failure rate >2% over 10 minutes.

## Testing Strategy
- **Unit Tests**: Adopt TUnit (open-source minimal testing framework) alongside FluentAssertions for expressive checks of domain logic.
- **Integration Tests**: Use Testcontainers (PostgreSQL, MinIO) with Respawn to reset state between tests.
- **Component Tests**: bUnit + Verify for Blazor components, ensuring responsive layouts and Simple/UI theme adherence.
- **End-to-End Tests**: Playwright .NET for browser automation, including mobile viewport simulations and accessibility audits (axe-core).
- **Contract Tests**: Validate Minimal API endpoints against generated OpenAPI specs using NSwag or Swashbuckle tooling.
- **Performance Testing**: Benchmark critical paths (search, document rendering) with BenchmarkDotNet and load test using k6 (OSS) against containerized environment.

## Security Considerations
- Secrets sourced from Vault-compatible providers or environment variables injected through the hosting platform.
- Enforce TLS (HTTP/2 + HSTS) and strong CSP headers. Validate uploaded specs against JSON/YAML schema to prevent malicious payloads.
- Implement data encryption at rest (PostgreSQL TDE or pgcrypto) and field-level hashing for sensitive ownership contacts.

## SLA & Reliability Targets
- Availability objective: ≥99.5% monthly uptime with 4-hour monthly error budget.
- Recovery Time Objective (RTO): 30 minutes for catalog service; Recovery Point Objective (RPO): 15 minutes via incremental connector replays.
- Performance SLOs: portal response <2s p95, sync cycle <15 minutes for supported connectors, export generation within 5 minutes of change detection.
- Observability SLO reviews conducted quarterly with governance board to adjust thresholds as catalog scales.

## Risk Assessment & Mitigations
- **Component Support Lag**: Monitor Simple/UI releases; maintain abstraction to pivot to MudBlazor if .NET 10 support delays occur.
- **Connector Credential Complexity**: Centralize credential issuance through IdP app registrations and automate rotation scripts.
- **Sync Backlog Growth**: Implement adaptive throttling and concurrency limits per connector; scale worker replicas horizontally when backlog exceeds defined thresholds.
- **Self-Managed Infrastructure**: Provide infrastructure-as-code samples (Helm charts, docker-compose) but ensure documentation covers bare-metal deployments.

## Open Questions / TODOs
- Legal confirmation for Simple/UI usage in regulated environments (expected approval based on MIT license).
- Finalize governance-approved SLA reporting format and escalation paths.
- Determine preferred object storage solution (MinIO vs vendor-managed) for specification archives in each deployment environment.

## References & Resources
- SysinfoFocus Simple/UI documentation and sample gallery (free-to-use statement).
- Microsoft Learn: Blazor and Minimal APIs in ASP.NET Core (.NET 8+ preview features for .NET 10).
- OpenTelemetry .NET getting started guides and CNCF observability playbooks.
- TUnit GitHub repository and docs for modern .NET testing.
- Testcontainers for .NET, Playwright .NET documentation, NSwag tooling guides.
