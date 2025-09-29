namespace AsyncScapeIA.Application.Catalog;

using AsyncScapeIA.Domain;

public interface ICatalogQueryService
{
    Task<IReadOnlyList<ArchitectureComponent>> SearchComponentsAsync(string query, CancellationToken cancellationToken);
    Task<ArchitectureComponent?> GetComponentBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<SchemaDocument>> GetSchemasForComponentAsync(string componentSlug, CancellationToken cancellationToken);
}

