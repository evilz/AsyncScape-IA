namespace AsyncScapeIA.Infrastructure.Persistence.Configurations;

using AsyncScapeIA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SchemaDocumentEntityTypeConfiguration : IEntityTypeConfiguration<SchemaDocument>
{
    public void Configure(EntityTypeBuilder<SchemaDocument> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Checksum).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SpecificationBlobKey).HasMaxLength(300);
        builder.Property(x => x.DiscoveredAtUtc).IsRequired();
    }
}

