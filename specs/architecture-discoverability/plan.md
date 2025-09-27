# Implementation Plan: Architecture Discoverability Hub

**Branch**: `feature/architecture-discoverability-hub` | **Date**: 2025-09-27 | **Spec**: /specs/architecture-discoverability/spec.md
**Input**: Feature specification from `/specs/architecture-discoverability/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   - If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   - Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   - Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   - If violations exist: Document in Complexity Tracking
   - If no justification possible: ERROR "Simplify approach first"
   - Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 - research.md
   - If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 - contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   - If new violations: Refactor design, return to Phase 1
   - Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 - Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Architecture Discoverability Hub delivers a Blazor-based ASP.NET Core portal that centralizes architecture components, schemas, and ownership metadata pulled from OpenAPI/AsyncAPI sources via pluggable connectors, operates with provider-agnostic infrastructure, and achieves deep observability using OpenTelemetry (optionally orchestrated with .NET Aspire for development) while keeping the service topology intentionally minimal (WebApp, Worker, PostgreSQL, storage, identity, telemetry).

## Technical Context
**Language/Version**: .NET 10 (ASP.NET Core, C# latest)  
**Primary Dependencies**: Blazor Server, EF Core 10, Npgsql provider, SysinfoFocus Simple/UI (MIT), Polly, OpenTelemetry, .NET Aspire (dev orchestration), Microsoft.Identity.Web (OpenID Connect), AutoMapper  
**Scheduling**: Built-in `BackgroundService` with `PeriodicTimer`, `Channel`, and `IHostedLifecycleService` (no third-party scheduler)  
**Storage**: PostgreSQL 16 for operational data; S3-compatible object storage (e.g., MinIO) for specification assets  
**Testing**: TUnit + FluentAssertions (unit), bUnit + Verify (component), Testcontainers + Respawn (integration), Playwright .NET + axe-core (end-to-end), BenchmarkDotNet/k6 (performance)  
**Target Platform**: Containerized deployment (Docker/OCI) suitable for Kubernetes, Nomad, Docker Swarm, or self-managed hosts; no dependency on specific cloud vendor  
**Performance Goals**: 95% schema sync <15 min, search responses <2s for 10k components, override propagation before next export window (≤15 min)  
**Constraints**: All libraries must be free/open source; hosting must remain provider-agnostic; enforce async-first patterns, OpenTelemetry instrumentation, 24-month audit retention, responsive mobile UX, and minimal service footprint (avoid additional infrastructure until metrics justify)  
**Scale/Scope**: Catalog up to 10k components, dozens of connectors, ~500 internal users, exports consumed by downstream tooling

## Constitution Check
- **Clean Architecture Boundaries**: Maintain Domain, Application, Infrastructure, Web (UI/API), and optional Aspire host with DI-managed interfaces; keep domain logic free of framework dependencies. (PASS)
- **Async & Resilient Execution**: Use async/await, cancellation tokens, Polly resilience pipelines, and connector-level retry/backoff with circuit breakers. (PASS)
- **Automated Quality Gates**: CI runs dotnet format, analyzers, security scans, TUnit/bUnit/Testcontainers/Playwright suites with coverage enforcement; build fails on violations. (PASS)
- **Operational Observability**: Instrument via OpenTelemetry (traces/metrics/logs), propagate correlation IDs, expose health/readiness/startup endpoints, and provide Prometheus/Grafana dashboards. (PASS)
- **Security & Compliance by Default**: Integrate OpenID Connect provider (Azure AD, Keycloak, Okta, etc.), manage secrets via Vault-compatible stores, enable nullable + analyzers with warnings-as-errors, and run dependency vulnerability scans. (PASS)
- **Technical Standards**: Target .NET 10 LTS, implicit usings, editorconfig + dotnet format, multi-stage container builds with image scanning (Trivy/grype). (PASS)
- **Delivery Workflow**: Follow trunk-based branching (`feature/{ticket}`), require domain-qualified review, apply semantic versioning, automate staging deployments using Aspire pipeline scripts or lightweight rollout manifests with smoke-tested promotions. (PASS)
- **Governance Alignment**: Record constitution exceptions with expiry, publish monthly SLA reports to the governance board, note governance impacts in release notes, and run quarterly compliance audits on SLA metrics. (PASS)

## Project Structure

### Documentation (this feature)
```
specs/architecture-discoverability/
|-- plan.md              # This plan (/plan output)
|-- research.md          # Phase 0 deliverable
|-- data-model.md        # Phase 1 deliverable
|-- quickstart.md        # Phase 1 deliverable
|-- contracts/           # Phase 1 deliverables
`-- tasks.md             # Phase 2 output (/tasks command)
```

### Source Code (repository root)
```
src/
|-- AsyncScapeIA.Domain/         # Entities, value objects, domain events
|-- AsyncScapeIA.Application/    # Use cases, DTOs, validators, policies
|-- AsyncScapeIA.Infrastructure/ # EF Core context, repository abstractions, external adapters
|-- AsyncScapeIA.Integrations/   # Connector SDK + provider implementations
|-- AsyncScapeIA.WebApp/         # Blazor UI + minimal APIs, DI composition
|-- AsyncScapeIA.Shared/         # Cross-layer contracts and helpers

src/AsyncScapeIA.Workers/        # BackgroundService-based schedulers (native .NET)
Aspire.AppHost/                  # Optional dev-time orchestration via Aspire (not required in prod)
Aspire.ServiceDefaults/          # Shared telemetry/health defaults used in dev/local environments

public-api/                      # Optional export-focused minimal API project (if separated from UI)

tests/
|-- AsyncScapeIA.UnitTests/      # TUnit suites
|-- AsyncScapeIA.ApplicationTests/
|-- AsyncScapeIA.IntegrationTests/
|-- AsyncScapeIA.ComponentTests/ # bUnit + Verify
`-- AsyncScapeIA.UITests/        # Playwright
```

**Structure Decision**: Retain clean architecture separation with layered projects, provide optional Aspire-based composition for local/dev convenience, and ensure production deployment is container-first and environment agnostic.

## Phase 0: Outline & Research
1. Confirm licensing, accessibility compliance, and mobile readiness for SysinfoFocus Simple/UI; document theming strategy.
2. Catalogue OpenAPI/AsyncAPI connector targets (Confluent, GitHub, Azure API Management, internal registries) and required auth flows using OpenID Connect-issued credentials.
3. Design native scheduling approach with BackgroundService + PeriodicTimer, including concurrency controls and resilience policies.
4. Define storage strategy: PostgreSQL schema management, seed data approach, S3-compatible object storage configuration with retention policies.
5. Establish identity strategy with chosen OpenID Connect provider (roles, scopes, MFA expectations) and secrets management via Vault-compatible tooling.
6. Assemble observability stack: OpenTelemetry exporters, Prometheus/Grafana dashboards, Jaeger/Loki integration, and alert thresholds supporting SLA targets.
7. Document conflict resolution workflow, SLA reporting cadence (availability, RPO/RTO), and governance review process.
8. Verify all tooling (test frameworks, security scanners, container toolchain) meet open-source/free usage requirements.

**Output**: research.md summarizing decisions, risks, and remaining assumptions (COMPLETED).

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Domain Modeling** (`data-model.md`): Capture Architecture Component, Connector Configuration, Schema Document, Ownership Record, Sync Job plus supporting value objects (identifiers, versioning, contact info, audit metadata).
2. **Application Layer Design**: Define use case handlers for catalog navigation, connector lifecycle, sync orchestration, override approvals, export production; specify validation rules and authorization policies mapped to OpenID Connect roles.
3. **Connector Contract Definition** (`contracts/`): Draft provider-agnostic interfaces (`IConnectorProvider`, `IConnectorAuthenticator`) and message schemas; produce OpenAPI/AsyncAPI specs for export APIs and webhook integrations.
4. **Persistence Plan**: Outline EF Core DbContext, entity configurations, migrations + seeding strategy, audit partitioning, and retention enforcement BackgroundService.
5. **Sync Orchestration Design**: Detail BackgroundService architecture (PeriodicTimer cadence, Channel queues, telemetry hooks), resilience policies, and graceful shutdown handling.
6. **UI/UX Blueprint**: Map Blazor component hierarchy leveraging Simple/UI, responsive layouts, search/filter UX, documentation viewer flows; include major interactions in quickstart.md.
7. **Security & Observability Specification**: Document OpenID Connect setup, scopes, token flows, secrets storage, and OpenTelemetry configuration (trace/span naming, metric dimensions, logging strategy).
8. **Deployment Topology**: Document Aspire-first deployment topology, container packaging, and map the minimal service set onto varied infrastructures without vendor lock-in.
9. **Diagram & Test Scaffolds**: Capture system interaction diagrams (e.g., Mermaid sequence) and author failing unit/contract/E2E test scaffolds to guide implementation.
10. **Agent Context Update**: After producing design artifacts, run  `.specify/scripts/powershell/update-agent-context.ps1 -AgentType codex` so the tooling reflects chosen technologies. 
**Output**: data-model.md, contracts, quickstart.md, deployment-topology.md, sequence-diagram.mmd, tests.md, initial failing tests, Aspire notes, updated agent context.
## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Base on `.specify/templates/tasks-template.md`.
- Generate tasks spanning domain modeling, application services, connectors, infrastructure, UI, background scheduling, observability, CI/CD, and governance reporting.
- Mark `[P]` for independent work streams while respecting dependency ordering.
- Include tasks for migrations, seed automation, identity integration, Aspire configuration, and SLA instrumentation.

**Ordering Strategy**:
- Follow TDD: write failing tests/contracts prior to implementation.
- Flow by dependency: Domain → Application → Infrastructure → Integrations → Background services → Web UI → Export APIs.
- Build observability and security alongside functional deliverables; avoid deferred hardening phases.

**Planned Task Breakdown**:
1. Bootstrap solution structure with Domain/Application/Infrastructure/Integrations/WebApp/Workers/Shared projects and shared analyzers.
2. Configure `.editorconfig`, nullable, warnings-as-errors, and baseline analyzer rules across all projects.
3. Implement domain entities and value objects (ArchitectureComponent, SchemaDocument, OwnershipRecord, SyncJob) with invariants. `[P]`
4. Author failing TUnit unit tests validating domain invariants and ownership requirements. `[P]`
5. Implement application layer services for catalog queries, overrides, and exports with FluentValidation policies.
6. Create Testcontainers-based integration harness (PostgreSQL + MinIO) with failing repository tests.
7. Implement EF Core DbContext, configurations, and initial migrations aligned to data-model.md.
8. Add idempotent seed service loading sample components, ownership groups, and connectors on first run.
9. Build object storage gateway abstraction with MinIO provider for specification assets. `[P]`
10. Implement audit retention BackgroundService enforcing 24-month policy via partition cleanup.
11. Deliver connector SDK abstractions (`IConnectorProvider`, cursor persistence, resilience policies).
12. Implement GitHub connector sourcing OpenAPI specs via authenticated HTTP client.
13. Implement Confluent Schema Registry connector with artifact filtering and checksum support. `[P]`
14. Implement Azure API Management connector leveraging Azure SDK pagination. `[P]`
15. Author connector contract tests (happy path, auth failure, rate limiting) using recorded fixtures.
16. Implement worker scheduling pipeline (PeriodicTimer + Channel) with telemetry hooks.
17. Wire manual sync trigger flow and conflict resolution pipeline across application/worker layers.
18. Expose export Minimal APIs matching `contracts/export-api.yaml` with pagination + authorization.
19. Compose DI root in WebApp registering domain/application/infrastructure services.
20. Build Simple/UI shell (navigation, layout, theming) with responsive breakpoints. `[P]`
21. Implement catalog search/filter experience with debounced queries and result cards.
22. Implement schema documentation viewer rendering OpenAPI/AsyncAPI content. `[P]`
23. Implement ownership management UI for mapping components to groups with audit visibility.
24. Integrate OpenID Connect auth (Microsoft.Identity.Web) and map roles to authorization policies.
25. Define Aspire AppHost resources (WebApp, Workers, PostgreSQL, storage, identity, OTEL) plus ServiceDefaults.
26. Instrument WebApp and Workers with OpenTelemetry traces, metrics, logs, and health endpoints.
27. Author failing Playwright E2E scenarios (login, catalog search, manual sync validation).
28. Add CI pipeline script executing analyzers, tests, coverage, Playwright, and container scans.
29. Document governance SLA reporting workflow and automate monthly report template. `[P]`
30. Update operational documentation (quickstart, connector guides, runbooks) with Aspire usage notes.

**Estimated Output**: 25-30 numbered tasks with owners, dependencies, and `[P]` markers for parallelizable items.

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan
## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `/memory/constitution.md`*
















