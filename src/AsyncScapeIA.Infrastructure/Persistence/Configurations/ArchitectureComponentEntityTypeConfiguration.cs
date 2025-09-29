namespace AsyncScapeIA.Infrastructure.Persistence.Configurations;

using AsyncScapeIA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ArchitectureComponentEntityTypeConfiguration : IEntityTypeConfiguration<ArchitectureComponent>
{
    public void Configure(EntityTypeBuilder<ArchitectureComponent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Domain).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.Ignore(x => x.OwnershipAssignments);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

