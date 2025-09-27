# Connector Contract Specification

## Purpose
Defines the provider-agnostic interface that all connectors must implement to ingest OpenAPI/AsyncAPI assets, ownership hints, and metadata into the Architecture Discoverability Hub.

## Interface Overview
```csharp
public interface IConnectorProvider
{
    ConnectorDescriptor Descriptor { get; }
    Task<ConnectorHealth> CheckHealthAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<ConnectorArtifact> DiscoverArtifactsAsync(ConnectorCursor cursor, CancellationToken cancellationToken);
    Task<ConnectorCursor> PersistCursorAsync(ConnectorCursor cursor, CancellationToken cancellationToken);
}
```

### ConnectorDescriptor
- `ConnectorType` (enum)
- `DisplayName`
- `Capabilities` (flags: SupportsOwnershipHints, ProvidesAsyncApi, ProvidesOpenApi, SupportsDeltaSync)
- `DefaultSyncInterval`

### ConnectorArtifact
- `ArtifactId`
- `Name`
- `Format` (OpenAPI | AsyncAPI)
- `Version`
- `SourceUri`
- `ContentStream` (binary)
- `Checksum`
- `OwnershipHints` (collection of `OwnershipHint` value objects)
- `ComponentRelationships` (list of component identifiers or tags)

### OwnershipHint
- `ExternalGroupId`
- `Confidence` (0-1)
- `Role` (Primary | Secondary | Escalation)

### ConnectorCursor
- `ConnectorConfigurationId`
- `Position` (string opaque token)
- `CapturedAtUtc`

### ConnectorHealth
- `Status` (Healthy | Degraded | Unhealthy)
- `Details` (string)
- `Latency` (TimeSpan)
- `LastSuccessfulSync`

## Responsibilities
- Connectors must provide idempotent artifact discovery based on cursor state.
- Connectors stream artifacts via `IAsyncEnumerable` to avoid memory spikes.
- Ownership hints are optional; connectors should populate when source metadata exists.
- Connectors persist updated cursors only after artifacts succeed processing.

## Error Handling
- Throw `ConnectorAuthenticationException` when credentials invalid.
- Throw `ConnectorRateLimitException` when upstream quota exceeded; include `RetryAfter` hint.
- Unknown failures bubble as `ConnectorTransientException` to trigger Polly retries.

## Configuration Schema (JSON)
```json
{
  "endpoint": "https://api.example.com",
  "auth": {
    "mode": "ClientCredentials",
    "credentialRef": "secrets/connector/sample"
  },
  "filters": {
    "projects": ["payments", "accounts"],
    "tags": ["domain:core"]
  }
}
```

## Webhook Support (Optional)
If upstream systems support outbound notifications, connectors may expose webhook handlers under `/connectors/{connectorId}/webhook`. Payloads must include:
- `eventType`
- `artifactId`
- `timestamp`
- `signature`

Webhook handlers validate signature and enqueue immediate sync for affected artifact.

## Security Considerations
- Credentials fetched via secret store reference defined in configuration.
- All outbound HTTP requests must honor cancellation tokens and enforce TLS 1.2+.
- Sensitive data (API keys) never logged; health checks redact secrets.

## Testing Requirements
- Provide contract tests that exercise happy path, pagination, authentication failure, and rate limiting scenarios.
- Capture sample artifacts in `tests/fixtures/connectors/<provider>/` for deterministic testing.

## Extensibility
- Support custom metadata by extending `ConnectorArtifact.Properties` (dictionary string/object) while keeping base schema stable.
- Publish provider-specific guidance in `/docs/connectors/<type>.md`.
