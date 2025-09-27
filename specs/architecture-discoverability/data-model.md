# Data Model: Architecture Discoverability Hub

## Overview
The solution maintains a normalized relational model in PostgreSQL backed by EF Core. Entities capture architecture components, ingested schema documents, ownership metadata, connector configurations, and synchronization job history. Value objects encapsulate identifiers, versioning, and contact details to protect invariants. Audit trails track mutations for 24 months.

## Core Entities
### ArchitectureComponent
- `ArchitectureComponentId` (Guid)
- `Slug` (string, unique)
- `Name` (string, required, <=200 chars)
- `Description` (string)
- `Domain` (string)
- `LifecycleState` (enum: Proposed, Active, Deprecated, Retired)
- `BusinessCriticality` (enum: Low, Medium, High)
- `PrimaryOwnerId` (Guid reference to OwnershipRecord)
- `CreatedAtUtc` / `UpdatedAtUtc`
- Navigation: `OwnershipAssignments` (many), `SchemaLinks` (many), `Dependencies` (self-referencing many-to-many via `ComponentDependency`)

### ComponentDependency
- `SourceComponentId` (Guid)
- `TargetComponentId` (Guid)
- `DependencyType` (enum: Upstream, Downstream, Bidirectional)
- `Rationale` (string)

### SchemaDocument
- `SchemaDocumentId` (Guid)
- `Name` (string)
- `Format` (enum: OpenAPI, AsyncAPI)
- `Version` (string)
- `SourceUri` (string)
- `Checksum` (string)
- `SpecificationBlobKey` (string reference to object storage path)
- `DiscoveredAtUtc`
- `LastSyncedAtUtc`
- `OwningConnectorId` (Guid reference to ConnectorConfiguration)
- Navigation: `LinkedComponents` (many-to-many via `ComponentSchemaLink`)

### ComponentSchemaLink
- `ArchitectureComponentId` (Guid)
- `SchemaDocumentId` (Guid)
- `Relationship` (enum: Produces, Consumes, Documents)
- `Confidence` (decimal 0-1)

### ConnectorConfiguration
- `ConnectorConfigurationId` (Guid)
- `Name` (string)
- `ConnectorType` (enum: ConfluentRegistry, GitHubRepo, AzureApiManagement, InternalHttp)
- `Endpoint` (string)
- `AuthenticationMode` (enum: ClientCredentials, ApiKey, PersonalAccessToken)
- `CredentialReference` (string key to secret store)
- `SyncInterval` (TimeSpan)
- `IsEnabled` (bool)
- `ApprovalStatus` (enum: Draft, PendingReview, Approved, Revoked)
- `LastSyncStatus` (enum: Success, Failed, Warning)
- `LastSyncAtUtc`
- `CreatedByUserId` (string external IdP subject)
- `Metadata` (JSONB for provider-specific settings)
- Navigation: `SyncJobs` (many)

### SyncJob
- `SyncJobId` (Guid)
- `ConnectorConfigurationId` (Guid)
- `StartedAtUtc`
- `CompletedAtUtc`
- `Status` (enum: Succeeded, Failed, Partial, Cancelled)
- `ItemsProcessed` (int)
- `ConflictsDetected` (int)
- `ErrorSummary` (string, nullable)
- `CorrelationId` (Guid)
- `TriggeredBy` (enum: Scheduled, Manual, API)

### OwnershipRecord
- `OwnershipRecordId` (Guid)
- `ExternalGroupId` (string from IdP)
- `DisplayName` (string)
- `PrimaryContact` (value object, see below)
- `EscalationContacts` (JSON array of Contact value objects)
- `EffectiveFromUtc`
- `EffectiveToUtc` (nullable)
- Navigation: `AssignedComponents` (many), `AuditEntries` (many)

### OverrideRequest
- `OverrideRequestId` (Guid)
- `TargetEntityType` (enum: ArchitectureComponent, SchemaDocument, OwnershipRecord)
- `TargetEntityId` (Guid)
- `FieldName` (string)
- `OriginalValue` (string)
- `ProposedValue` (string)
- `Justification` (string)
- `Status` (enum: Pending, Approved, Rejected, Applied)
- `ReviewedByUserId` (string)
- `ReviewedAtUtc`
- `CreatedByUserId` (string)
- `CreatedAtUtc`

### AuditEntry
- `AuditEntryId` (Guid)
- `EntityType` (string)
- `EntityId` (Guid)
- `Action` (enum: Created, Updated, Deleted, OverrideApplied)
- `ChangedByUserId` (string)
- `ChangedAtUtc`
- `ChangeSummary` (JSONB capturing field-level diffs)
- `CorrelationId`

## Value Objects
- **Contact**: `Name`, `Email`, `Phone`, `TimeZone` (validates format, ensures required for escalation tiers).
- **OwnershipAssignment**: encapsulates component-to-owner mapping with `Role` (enum: Primary, Secondary, Escalation), `OwnershipRecordId`.
- **SyncCursor**: stores pagination/delta positions per connector (`CursorType`, `CursorValue`, `CapturedAtUtc`).

## Enumerations
- `LifecycleState`, `BusinessCriticality`, `DependencyType`, `SchemaFormat`, `ComponentSchemaRelationship`, `ConnectorType`, `AuthenticationMode`, `ApprovalStatus`, `LastSyncStatus`, `SyncStatus`, `OverrideStatus`, `TriggerSource`.

## Relationships Diagram (Textual)
- ArchitectureComponent 1..* OwnershipAssignment -> OwnershipRecord (many-to-one).
- ArchitectureComponent <-> SchemaDocument via ComponentSchemaLink (many-to-many).
- ConnectorConfiguration 1..* SyncJob.
- ConnectorConfiguration 1..* SchemaDocument.
- OverrideRequest targets different entity types by Id.
- AuditEntry links to entity by type + Id (no direct FK to allow polymorphism).

## Invariants & Business Rules
- Components must have at least one ownership assignment with Role = Primary.
- ConnectorConfiguration can be Enabled only when ApprovalStatus = Approved.
- SyncJob.Status cannot be Succeeded if ConflictsDetected > 0; such cases are Partial.
- OverrideRequest transitions: Pending -> Approved/Rejected; Approved -> Applied after change persisted.
- SchemaDocument.Checksum ensures idempotent imports; duplicates skip re-processing.

## Storage Considerations
- Use PostgreSQL schemas: `catalog` (domain tables), `audit` (AuditEntry partitions), `integration` (SyncJob, ConnectorConfiguration).
- Partition `audit.AuditEntry` monthly; maintain index on `EntityId` and `ChangedAtUtc`.
- Store `SchemaDocument` content in object storage; database only retains metadata and blob key.
- Enable EF Core concurrency tokens (`RowVersion` bytea) on mutable tables (ArchitectureComponent, OwnershipRecord, SchemaDocument).

## Future Extensions
- Add `Capability` entity to capture business capabilities and map components.
- Support hierarchical domains via self-referential Domain table if needed.
- Introduce tagging system (key/value) for components and schemas.
