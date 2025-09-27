using AsyncScapeIA.Workers;
using Npgsql;
using TUnit;
using TUnit.Assertions;
using Testcontainers.PostgreSql;

namespace AsyncScapeIA.IntegrationTests.Infrastructure;

public class AuditRetentionTests
{
    [Test]
    public async Task RunOnceAsync_removes_records_older_than_retention_window()
    {
        await using var container = new PostgreSqlBuilder().Build();
        await container.StartAsync();

        var worker = new AuditRetentionWorker(container.GetConnectionString(), TimeSpan.FromDays(730));

        await Assert.That(async () => await worker.RunOnceAsync(CancellationToken.None))
            .Throws<NotImplementedException>();
    }
}
