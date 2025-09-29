namespace AsyncScapeIA.Application.Catalog;

using AsyncScapeIA.Domain;

public sealed class CatalogQueryService : ICatalogQueryService
{
    private readonly ICatalogReadStore _readStore;

    public CatalogQueryService(ICatalogReadStore readStore)
    {
        _readStore = readStore;
    }

    public Task<IReadOnlyList<ArchitectureComponent>> SearchComponentsAsync(string query, CancellationToken cancellationToken)
        => _readStore.SearchComponentsAsync(query, cancellationToken);

    public Task<ArchitectureComponent?> GetComponentBySlugAsync(string slug, CancellationToken cancellationToken)
        => _readStore.GetComponentBySlugAsync(slug, cancellationToken);

    public Task<IReadOnlyList<SchemaDocument>> GetSchemasForComponentAsync(string componentSlug, CancellationToken cancellationToken)
        => _readStore.GetSchemasForComponentAsync(componentSlug, cancellationToken);
}

public interface ICatalogReadStore
{
    Task<IReadOnlyList<ArchitectureComponent>> SearchComponentsAsync(string query, CancellationToken cancellationToken);
    Task<ArchitectureComponent?> GetComponentBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<SchemaDocument>> GetSchemasForComponentAsync(string componentSlug, CancellationToken cancellationToken);
}

