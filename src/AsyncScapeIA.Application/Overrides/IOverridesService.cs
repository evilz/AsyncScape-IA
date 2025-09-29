namespace AsyncScapeIA.Application.Overrides;

using AsyncScapeIA.Domain;

public interface IOverridesService
{
    Task ApplyComponentOverrideAsync(string componentSlug, Action<ArchitectureComponent> apply, CancellationToken cancellationToken);
    Task ApplySchemaOverrideAsync(Guid schemaId, Action<SchemaDocument> apply, CancellationToken cancellationToken);
}

