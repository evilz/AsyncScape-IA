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
Architecture Discoverability Hub delivers a Blazor-based ASP.NET Core experience that harmonizes architecture components, schemas, and ownership metadata through automated connectors (including Azure sources), transforms OpenAPI/AsyncAPI assets into curated documentation, and operates with end-to-end observability via Aspire + OpenTelemetry and built-in .NET 10 background orchestration.

## Technical Context
**Language/Version**: .NET 10 (ASP.NET Core, C# latest)  
**Primary Dependencies**: Blazor (interactive server-side), EF Core 10, Npgsql provider, SysinfoFocus Simple/UI (MIT-licensed), Polly, OpenTelemetry, .NET Aspire (AppHost + ServiceDefaults), Azure SDK for .NET (Resource Graph, Event Grid, Service Bus), Microsoft.Identity.Web (Azure AD), AutoMapper  
**Scheduling**: Built-in `BackgroundService`/`PeriodicTimer` + `IHostedLifecycleService` (no third-party scheduler)  
**Storage**: PostgreSQL for operational persistence, Azure Blob Storage for uploaded specifications (encrypted, lifecycle-managed)  
**Testing**: xUnit, FluentAssertions, bUnit for Blazor, Respawn + Testcontainers for PostgreSQL integration testing, Playwright for end-to-end UI, Verify snapshots for documentation rendering  
**Target Platform**: Azure App Service (Linux) orchestrated via Aspire, backed by Azure PostgreSQL Flexible Server, Azure Monitor, and Application Insights compatible exporters  
**Performance Goals**: 95% schema sync in <15 minutes, search responses <2 seconds for 10k components, override propagation before next export window (≤15 minutes)  
**Constraints**: All UI/component libraries and SDKs must be free/open-source; connectors must include Azure integrations using open-source Azure SDKs; secrets managed via Key Vault; warnings-as-errors; responsive mobile-first layout; async cancellation across connectors; audit data retained 24 months  
**Scale/Scope**: Catalog up to 10k components, dozens of connectors (including Azure registries), ~500 internal users, downstream consumers via secured APIs/exports

## Constitution Check
- **Clean Architecture Boundaries**: Maintain Domain, Application, Infrastructure, Web, and Aspire-hosted composition with contracts between layers and DI; domain code stays framework-agnostic. (PASS)
- **Async & Resilient Execution**: Employ async/await, cancellation tokens, Polly resilience policies, and Azure SDK retries across connectors and background orchestration. (PASS)
- **Automated Quality Gates**: CI executes dotnet format, analyzers, security scans, test matrix (unit, integration, component, Playwright) with coverage enforcement and build failure on violations. (PASS)
- **Operational Observability**: Leverage Aspire’s ServiceDefaults with OpenTelemetry tracing/logging/metrics, correlation IDs on sync flows, health/readiness probes, and Azure Monitor dashboards/runbooks. (PASS)
- **Security & Compliance by Default**: Integrate Microsoft.Identity.Web with Azure AD roles, protect secrets via Key Vault, enforce nullable + analyzers with warnings-as-errors, run dependency vulnerability scans. (PASS)
- **Technical Standards**: Target .NET 10 LTS, use implicit usings, editorconfig + dotnet format, multi-stage container builds generated via Aspire with security scanning aligned to constitution. (PASS)
- **Delivery Workflow**: Follow trunk-based branching (`feature/{ticket}`), domain-qualified reviews, semantic versioning, Aspire-managed staging deployments with smoke-tested promotion to production. (PASS)
- **Governance Alignment**: Track constitution exceptions with expiry, document governance impacts in release notes, schedule quarterly compliance audits post-launch. (PASS)

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
|-- AsyncScapeIA.Infrastructure/ # EF Core context, repositories, Azure SDK integrations
|-- AsyncScapeIA.Integrations/   # Connector SDK + provider implementations
|-- AsyncScapeIA.WebApp/         # Blazor UI, minimal APIs, DI composition
|-- AsyncScapeIA.Shared/         # Cross-layer contracts and helpers

src/AsyncScapeIA.Workers/        # Hosted services using BackgroundService + PeriodicTimer (no external scheduler)
Aspire.AppHost/                  # Aspire orchestration project (environment composition)
Aspire.ServiceDefaults/          # Shared Aspire defaults (OpenTelemetry, health)

public-api/                      # Export-focused minimal API (if separated from UI)

tests/
|-- AsyncScapeIA.UnitTests/
|-- AsyncScapeIA.ApplicationTests/
|-- AsyncScapeIA.IntegrationTests/
|-- AsyncScapeIA.ComponentTests/ # bUnit
`-- AsyncScapeIA.UITests/        # Playwright
```

**Structure Decision**: Adopt clean architecture boundaries with dedicated projects per layer, coordinate deployments through Aspire (AppHost + ServiceDefaults), and host scheduling logic within first-party worker/hosted services using built-in .NET 10 primitives—no external schedulers.

## Phase 0: Outline & Research
1. Confirm licensing and accessibility compliance for SysinfoFocus Simple/UI (ensure MIT or equivalent, responsive capabilities) and document theming strategy for mobile responsiveness.
2. Catalogue Azure and non-Azure connector targets; document authentication flows using Azure SDKs (Azure Resource Graph, Event Grid, Service Bus, GitHub, Confluent) ensuring all client libraries are open source and cost-free.
3. Design background orchestration using only .NET 10 primitives (BackgroundService, PeriodicTimer, Task.Delay); evaluate `IHostedLifecycleService` for graceful startup/shutdown and load shedding without external dependencies.
4. Define storage and lifecycle for uploaded specifications in Azure Blob Storage with encryption, retention, and clean-up aligned to audit policy.
5. Establish Azure AD app registration, role assignments (administrator, curator, contributor, viewer), and MFA/conditional access posture using Microsoft.Identity.Web (open source).
6. Configure Aspire environment: determine component manifests, OpenTelemetry exporters (OTLP/Prometheus), log sinks, dashboards, and alert thresholds for sync latency/failures.
7. Document conflict resolution and override workflow, including notification channels, auditing, and integration with governance board expectations.
8. Identify any third-party libraries still required and verify they meet open-source/free criteria; replace closed-source options before design proceeds.

**Output**: research.md summarizing decisions, risks, and finalized assumptions ready for Phase 1.

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Domain Modeling** (`data-model.md`): Model Architecture Component, Connector Configuration, Schema Document, Ownership Record, Sync Job, along with value objects for identifiers, versioning, contact data, and audit trails.
2. **Application Layer Design**: Define use case handlers for catalog browsing, connector management, sync orchestration, manual overrides, export generation; specify FluentValidation rules and policy-based authorization tied to Azure AD roles.
3. **Connector Contract Definition** (`contracts/`): Draft provider-agnostic interfaces (`IConnectorProvider`, `IConnectorAuthenticator`) leveraging Azure SDK abstractions; author OpenAPI/AsyncAPI specifications for export APIs and webhooks, including Azure-specific connector schemas.
4. **Persistence Plan**: Outline EF Core DbContext, entity configurations, migrations/seed strategy for initial data load; design audit log retention enforcement using PostgreSQL partitioning and periodic cleanup BackgroundService.
5. **Sync Orchestration Design**: Architect BackgroundService-based scheduler utilizing PeriodicTimer, linked cancellation tokens, and Channel-based job queues; design resilience with Polly policies and telemetry instrumentation for each stage.
6. **UI/UX Blueprint**: Map Blazor component tree integrating Simple/UI, responsive breakpoints, theming, search/filter UX, documentation viewer flow; capture key interactions in quickstart.md.
7. **Security & Observability Specification**: Detail Microsoft.Identity.Web integration, token acquisition flows, scope enforcement, and Aspire-provided OpenTelemetry configuration (trace/span naming, metric dimensions, log enrichment, health probes).
8. **Aspire Composition**: Define AppHost configuration (services, environment variables, resource references) and ServiceDefaults customizations (OpenTelemetry exporters, rate-limiting, retries) to standardize service hosting without extra dependencies.
9. **Agent Context Update**: After design artifacts are produced, run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType codex` to synchronize tooling context with new technologies and decisions.

**Output**: data-model.md, connector contracts, quickstart.md, Aspire composition notes, initial failing test scaffolds, and updated agent context as mandated.

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Base on `.specify/templates/tasks-template.md`.
- Derive tasks from Phase 1 assets covering domain entities, application services, connector providers (including Azure integrations), infrastructure (EF Core, Blob storage, audit retention), Blazor UI, BackgroundService scheduler, Aspire configuration, OpenTelemetry wiring, CI/CD updates.
- Mark `[P]` for independent efforts (e.g., specific connector implementations, UI modules) while respecting layer dependencies.
- Include explicit tasks for migrations/seed data, Azure AD app registration automation, Aspire environment scripts, and observability dashboards.

**Ordering Strategy**:
- Follow TDD: produce failing tests/contracts before coding features.
- Build vertically by dependency: Domain → Application → Infrastructure → Integrations → Background services → Web UI → Export APIs.
- Integrate observability/security tasks alongside functional increments (no deferred hardening).

**Estimated Output**: 25-30 numbered tasks with owners, dependencies, and `[P]` markers for parallel-ready work streams.

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
- [ ] Phase 0: Research complete (/plan command)
- [ ] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [ ] Post-Design Constitution Check: PASS
- [ ] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `/memory/constitution.md`*
