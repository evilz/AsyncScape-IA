namespace AsyncScapeIA.Infrastructure.Bootstrap;

using AsyncScapeIA.Domain;
using AsyncScapeIA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class SeedDataInitializer
{
    private readonly AppDbContext _db;
    private readonly ILogger<SeedDataInitializer> _logger;

    public SeedDataInitializer(AppDbContext db, ILogger<SeedDataInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        if (!await _db.Components.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Seeding initial architecture components...");
            var component = ArchitectureComponent.Create("Payments API", "Payments", LifecycleState.Active);
            _db.Components.Add(component);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

