namespace AsyncScapeIA.Application.Overrides;

using AsyncScapeIA.Domain;

public sealed class OverridesService : IOverridesService
{
    private readonly IOverridesRepository _repository;

    public OverridesService(IOverridesRepository repository)
    {
        _repository = repository;
    }

    public async Task ApplyComponentOverrideAsync(string componentSlug, Action<ArchitectureComponent> apply, CancellationToken cancellationToken)
    {
        var component = await _repository.GetComponentBySlugAsync(componentSlug, cancellationToken)
            ?? throw new InvalidOperationException($"Component '{componentSlug}' not found");

        apply(component);
        await _repository.SaveComponentAsync(component, cancellationToken);
    }

    public async Task ApplySchemaOverrideAsync(Guid schemaId, Action<SchemaDocument> apply, CancellationToken cancellationToken)
    {
        var schema = await _repository.GetSchemaByIdAsync(schemaId, cancellationToken)
            ?? throw new InvalidOperationException($"Schema '{schemaId}' not found");

        apply(schema);
        await _repository.SaveSchemaAsync(schema, cancellationToken);
    }
}

public interface IOverridesRepository
{
    Task<ArchitectureComponent?> GetComponentBySlugAsync(string slug, CancellationToken cancellationToken);
    Task SaveComponentAsync(ArchitectureComponent component, CancellationToken cancellationToken);

    Task<SchemaDocument?> GetSchemaByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveSchemaAsync(SchemaDocument schema, CancellationToken cancellationToken);
}

