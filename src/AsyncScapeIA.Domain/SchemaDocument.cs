namespace AsyncScapeIA.Domain;

public enum SchemaFormat
{
    OpenApi,
    AsyncApi
}

public sealed class SchemaDocument
{
    private SchemaDocument(Guid id, string name, SchemaFormat format, string version, string checksum)
    {
        Id = id;
        Name = name;
        Format = format;
        Version = version;
        Checksum = checksum;
    }

    public Guid Id { get; }

    public string Name { get; }

    public SchemaFormat Format { get; }

    public string Version { get; }

    public string Checksum { get; }

    public static SchemaDocument Create(string name, SchemaFormat format, string version, string checksum)
    {
        return new SchemaDocument(Guid.NewGuid(), name, format, version, checksum);
    }

    public bool MatchesChecksum(string checksum)
    {
        throw new NotImplementedException();
    }
}