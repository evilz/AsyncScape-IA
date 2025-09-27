namespace AsyncScapeIA.Integrations.Abstractions;

using System.Collections.Generic;
using AsyncScapeIA.Domain;

public enum ConnectorProvider
{
    GitHubRepository,
    ConfluentSchemaRegistry,
    AzureApiManagement
}

public sealed record ConnectorRequest(
    Guid ConnectorConfigurationId,
    ConnectorProvider Provider,
    string ExternalIdentifier,
    DateTimeOffset? Since,
    IReadOnlyDictionary<string, string?> Metadata);

public sealed record ConnectorComponent(
    string Slug,
    string Name,
    string Domain,
    LifecycleState LifecycleState,
    IReadOnlyDictionary<string, string?> Tags);

public sealed record ConnectorSchema(
    string Name,
    SchemaFormat Format,
    string Version,
    Uri SourceUri,
    string Checksum,
    string? OwningComponentSlug);

public sealed record ConnectorSyncResult(
    IReadOnlyList<ConnectorComponent> Components,
    IReadOnlyList<ConnectorSchema> Schemas,
    string? ContinuationToken,
    IReadOnlyList<ConnectorDiagnostic> Diagnostics);

public sealed record ConnectorDiagnostic(string Code, string Message, ConnectorDiagnosticSeverity Severity);

public enum ConnectorDiagnosticSeverity
{
    Information,
    Warning,
    Error
}

public interface IConnectorClient
{
    Task<ConnectorSyncResult> SyncAsync(ConnectorRequest request, CancellationToken cancellationToken);
}
