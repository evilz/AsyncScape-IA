namespace AsyncScapeIA.Infrastructure.Persistence.Configurations;

using AsyncScapeIA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class OwnershipRecordEntityTypeConfiguration : IEntityTypeConfiguration<OwnershipRecord>
{
    public void Configure(EntityTypeBuilder<OwnershipRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalGroupId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.Property(x => x.EffectiveToUtc);

        builder.OwnsOne(x => x.PrimaryContact, b =>
        {
            b.Property(c => c.Name).HasMaxLength(200).IsRequired();
            b.Property(c => c.Email).HasMaxLength(200).IsRequired();
            b.Property(c => c.Phone).HasMaxLength(50);
            b.Property(c => c.TimeZone).HasMaxLength(100);
        });

        builder.OwnsMany(typeof(Contact), "_escalationContacts", b =>
        {
            b.WithOwner().HasForeignKey("OwnershipRecordId");
            b.Property<int>("Id");
            b.HasKey("Id");
            b.Property<string>("Name").HasMaxLength(200).IsRequired();
            b.Property<string>("Email").HasMaxLength(200).IsRequired();
            b.Property<string?>("Phone").HasMaxLength(50);
            b.Property<string?>("TimeZone").HasMaxLength(100);
            b.ToTable("OwnershipRecordEscalationContacts");
        });
    }
}
