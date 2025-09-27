namespace AsyncScapeIA.Domain;

public enum SchemaFormat
{
    OpenApi,
    AsyncApi
}

public sealed class SchemaDocument
{
    private SchemaDocument(
        Guid id,
        string name,
        SchemaFormat format,
        string version,
        string checksum,
        Uri? sourceUri,
        string? specificationBlobKey,
        DateTimeOffset discoveredAtUtc,
        DateTimeOffset? lastSyncedAtUtc)
    {
        Id = id;
        Name = name;
        Format = format;
        Version = version;
        Checksum = checksum;
        SourceUri = sourceUri;
        SpecificationBlobKey = specificationBlobKey;
        DiscoveredAtUtc = discoveredAtUtc;
        LastSyncedAtUtc = lastSyncedAtUtc;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public SchemaFormat Format { get; private set; }

    public string Version { get; private set; }

    public string Checksum { get; private set; }

    public Uri? SourceUri { get; private set; }

    public string? SpecificationBlobKey { get; private set; }

    public DateTimeOffset DiscoveredAtUtc { get; }

    public DateTimeOffset? LastSyncedAtUtc { get; private set; }

    public static SchemaDocument Create(
        string name,
        SchemaFormat format,
        string version,
        string checksum,
        Uri? sourceUri = null,
        string? specificationBlobKey = null,
        DateTimeOffset? discoveredAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Schema document name is required");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new DomainRuleException("Schema document version is required");
        }

        if (string.IsNullOrWhiteSpace(checksum))
        {
            throw new DomainRuleException("Schema document checksum is required");
        }

        var timestamp = discoveredAtUtc ?? DateTimeOffset.UtcNow;

        return new SchemaDocument(
            Guid.NewGuid(),
            name.Trim(),
            format,
            version.Trim(),
            checksum.Trim(),
            sourceUri,
            string.IsNullOrWhiteSpace(specificationBlobKey) ? null : specificationBlobKey.Trim(),
            timestamp,
            null);
    }

    public bool MatchesChecksum(string checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum))
        {
            return false;
        }

        return string.Equals(Checksum, checksum.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void UpdateChecksum(string checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum))
        {
            throw new DomainRuleException("Checksum is required");
        }

        if (!MatchesChecksum(checksum))
        {
            Checksum = checksum.Trim();
            LastSyncedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    public void UpdateSourceUri(Uri sourceUri)
    {
        SourceUri = sourceUri ?? throw new ArgumentNullException(nameof(sourceUri));
        LastSyncedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateSpecificationBlobKey(string blobKey)
    {
        if (string.IsNullOrWhiteSpace(blobKey))
        {
            throw new DomainRuleException("Specification blob key is required");
        }

        SpecificationBlobKey = blobKey.Trim();
        LastSyncedAtUtc = DateTimeOffset.UtcNow;
    }
}
