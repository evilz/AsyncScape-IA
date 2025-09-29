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

        await using var connection = new NpgsqlConnection(container.GetConnectionString());
        await connection.OpenAsync();

        // Seed audit entries: two older than retention, one recent
        await using (var seed = connection.CreateCommand())
        {
            seed.CommandText = @"CREATE EXTENSION IF NOT EXISTS pgcrypto;
                                 CREATE TABLE IF NOT EXISTS audit_entries (
                                     id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                     occurred_at timestamptz NOT NULL
                                 );
                                 TRUNCATE TABLE audit_entries;";
            await seed.ExecuteNonQueryAsync();
        }

        await using (var insert = connection.CreateCommand())
        {
            insert.CommandText = @"INSERT INTO audit_entries(occurred_at) VALUES
                                   ((NOW() AT TIME ZONE 'UTC') - INTERVAL '900 day'),
                                   ((NOW() AT TIME ZONE 'UTC') - INTERVAL '800 day'),
                                   ((NOW() AT TIME ZONE 'UTC') - INTERVAL '100 day');";
            await insert.ExecuteNonQueryAsync();
        }

        var worker = new AuditRetentionWorker(container.GetConnectionString(), TimeSpan.FromDays(730));
        await worker.RunOnceAsync(CancellationToken.None);

        await using (var count = connection.CreateCommand())
        {
            count.CommandText = "SELECT COUNT(*) FROM audit_entries";
            var remaining = (long)(await count.ExecuteScalarAsync() ?? 0L);
            await Assert.That(remaining).IsEqualTo(1);
        }
    }
}
