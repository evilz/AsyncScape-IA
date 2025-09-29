# Tasks: Architecture Discoverability Hub

**Input**: Design documents from `/specs/architecture-discoverability/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Phase 3.1: Setup & Tooling
- [x] T001 Scaffold solution folders and .sln entries for `src/AsyncScapeIA.*`, `tests/AsyncScapeIA.*`, and Aspire projects per plan (`src/`, `tests/`, `Aspire.AppHost/`, `Aspire.ServiceDefaults/`).
- [x] T002 Configure shared `.editorconfig`, nullable, warnings-as-errors, and analyzer ruleset in `Directory.Build.props` / `Directory.Build.targets`.
- [x] T003 [P] Add root solution items for `global.json`, `NuGet.config`, and baseline package references (EF Core 10 preview, Npgsql, Simple/UI, OpenTelemetry).

## Phase 3.2: Tests First (TDD) – Must Fail Before Implementation
- [x] T004 Create TUnit suite `tests/AsyncScapeIA.UnitTests/Domain/ArchitectureComponentTests.cs` covering ownership invariants (should fail initially).
- [x] T005 [P] Add TUnit suite `tests/AsyncScapeIA.UnitTests/Domain/SchemaDocumentTests.cs` verifying checksum/idempotency rules (fails).
- [x] T006 Provision Testcontainers harness in `tests/AsyncScapeIA.IntegrationTests/Infrastructure/CatalogPersistenceTests.cs` asserting repository persistence (fails until repository exists).
- [x] T007 [P] Add Testcontainers-based audit retention test in `tests/AsyncScapeIA.IntegrationTests/Infrastructure/AuditRetentionTests.cs` (fails pending cleanup job).
- [x] T008 Create connector contract tests in `tests/AsyncScapeIA.ComponentTests/Connectors/GitHubConnectorTests.cs` using recorded fixtures (fails until connector).
- [x] T009 [P] Add Confluent connector contract tests in `tests/AsyncScapeIA.ComponentTests/Connectors/ConfluentConnectorTests.cs` (fails).
- [x] T010 Add Azure API Management connector contract tests in `tests/AsyncScapeIA.ComponentTests/Connectors/AzureApiManagementConnectorTests.cs` (fails).
- [x] T011 [P] Author Playwright scenario `tests/AsyncScapeIA.UITests/Scenarios/CatalogSearch.spec.cs` covering login & search (fails until UI).
- [x] T012 Add Playwright scenario `tests/AsyncScapeIA.UITests/Scenarios/ManualSync.spec.cs` validating sync workflow (fails).

## Phase 3.3: Core Domain & Application Implementation
- [x] T013 Implement domain entities/value objects in `src/AsyncScapeIA.Domain/` (ArchitectureComponent, SchemaDocument, OwnershipRecord, SyncJob) satisfying failing tests.
- [x] T014 Build application services in `src/AsyncScapeIA.Application/Catalog/` & `.../Overrides/` handling queries, overrides, exports.
- [x] T015 [P] Implement FluentValidation policies and authorization requirements in `src/AsyncScapeIA.Application/Validation/`.
- [x] T016 Create infrastructure persistence layer: DbContext + entity configurations in `src/AsyncScapeIA.Infrastructure/Persistence/` and initial EF Core migration.
- [x] T017 Add seed data service & startup hook in `src/AsyncScapeIA.Infrastructure/Bootstrap/SeedDataInitializer.cs`.
- [x] T018 [P] Implement object storage gateway with MinIO provider in `src/AsyncScapeIA.Infrastructure/Storage/`.
- [x] T019 Develop audit retention BackgroundService in `src/AsyncScapeIA.Workers/AuditRetentionWorker.cs`.
- [x] T020 Implement connector SDK abstractions in `src/AsyncScapeIA.Integrations/Abstractions/` with resilience policies.

## Phase 3.4: Connector Implementations & Scheduling
- [ ] T021 Implement GitHub connector provider in `src/AsyncScapeIA.Integrations/GitHub/GitHubConnectorProvider.cs`.
- [ ] T022 [P] Implement Confluent Schema Registry connector provider in `src/AsyncScapeIA.Integrations/Confluent/ConfluentConnectorProvider.cs`.
- [ ] T023 Implement Azure API Management connector provider in `src/AsyncScapeIA.Integrations/AzureApiManagement/AzureApiManagementConnectorProvider.cs`.
- [ ] T024 Build worker scheduling pipeline (PeriodicTimer + Channel) in `src/AsyncScapeIA.Workers/ConnectorScheduler.cs` with telemetry hooks.
- [ ] T025 Wire manual sync trigger & conflict resolution path across application + worker layers (command handlers, queue messages).

## Phase 3.5: Web UI & Minimal APIs
- [ ] T026 Compose DI root and minimal APIs in `src/AsyncScapeIA.WebApp/Program.cs` & `Startup/` modules, exposing endpoints declared in `contracts/export-api.yaml`.
- [ ] T027 Build Simple/UI shell (layout, navigation, theming) in `src/AsyncScapeIA.WebApp/Components/Layout/`.
- [ ] T028 [P] Implement catalog search & filters in `src/AsyncScapeIA.WebApp/Pages/Catalog.razor` with debounced queries.
- [ ] T029 [P] Implement schema documentation viewer in `src/AsyncScapeIA.WebApp/Pages/Schemas.razor` integrating OpenAPI/AsyncAPI rendering.
- [ ] T030 Implement ownership management UI in `src/AsyncScapeIA.WebApp/Pages/Ownership.razor` with audit visibility.
- [ ] T031 Integrate OpenID Connect authentication/authorization in `src/AsyncScapeIA.WebApp/Auth/` using Microsoft.Identity.Web.

## Phase 3.6: Observability, Aspire, and CI/CD
- [ ] T032 Define Aspire AppHost resources & ServiceDefaults in `Aspire.AppHost/Program.cs` and `Aspire.ServiceDefaults/`.
- [ ] T033 [P] Instrument WebApp & Workers with OpenTelemetry (tracing, metrics, logs) plus health checks in respective `Program.cs` files.
- [ ] T034 Add CI pipeline script `ops/pipelines/ci.yml` running analyzers, tests, coverage, Playwright, and container scans.
- [ ] T035 [P] Document SLA reporting automation (`ops/sla/sla-report-template.md`) and script placeholder `ops/sla/generate-report.ps1`.
- [ ] T036 Update operational docs: `README.md`, `specs/architecture-discoverability/quickstart.md`, connector guides, and runbook stubs with Aspire notes.

## Dependencies
- Tests (T004–T012) must be authored before related implementation tasks (T013+).
- T013 blocks T014, T015, T016.
- T016 blocks T017, T024, T026.
- T020 blocks connector implementations T021–T023.
- T024 & T025 depend on T016 and connector tasks (T021–T023).
- UI tasks T027–T030 depend on application services (T014) and export APIs (T026).
- Observability tasks T032–T033 depend on WebApp/Worker composition (T026, T024).
- Documentation task T036 is final and depends on completion of prior sections.

## Parallel Execution Examples
```
# Example 1: Run domain-focused tests in parallel
T004 (Domain ownership tests)
T005 (Schema document tests) [P]
T008 (GitHub connector tests)
T009 (Confluent connector tests) [P]

# Example 2: Parallel connector implementations after SDK ready
T021 (GitHub connector)
T022 (Confluent connector) [P]
T023 (Azure API Management connector)
```

## Notes
- `[P]` tasks operate on distinct files/modules and can run concurrently when dependencies satisfied.
- Ensure each test task fails before implementing corresponding feature work (strict TDD flow).
- Update task list status as progress occurs; treat tasks as checkpoints for commits.
- Governance/SLA automation (T035) should reference metrics captured via OpenTelemetry to support monthly reporting.









