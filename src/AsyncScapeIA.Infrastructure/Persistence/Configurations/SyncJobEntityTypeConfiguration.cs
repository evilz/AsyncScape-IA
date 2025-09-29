namespace AsyncScapeIA.Infrastructure.Persistence.Configurations;

using AsyncScapeIA.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SyncJobEntityTypeConfiguration : IEntityTypeConfiguration<SyncJob>
{
    public void Configure(EntityTypeBuilder<SyncJob> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.ItemsProcessed).IsRequired();
        builder.Property(x => x.ConflictsDetected).IsRequired();
        builder.Property(x => x.ErrorSummary).HasMaxLength(1000);
        builder.Property(x => x.ConnectorConfigurationId).IsRequired();
        builder.Property(x => x.TriggeredBy).IsRequired();
        builder.Property(x => x.CorrelationId).IsRequired();
    }
}
