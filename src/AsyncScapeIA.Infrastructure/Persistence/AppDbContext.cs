namespace AsyncScapeIA.Infrastructure.Persistence;

using AsyncScapeIA.Domain;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ArchitectureComponent> Components => Set<ArchitectureComponent>();
    public DbSet<SchemaDocument> Schemas => Set<SchemaDocument>();
    public DbSet<OwnershipRecord> OwnershipRecords => Set<OwnershipRecord>();
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

