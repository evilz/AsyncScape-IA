namespace AsyncScapeIA.Workers;

public sealed class AuditRetentionWorker
{
    public AuditRetentionWorker(string connectionString, TimeSpan retentionPeriod)
    {
        ConnectionString = connectionString;
        RetentionPeriod = retentionPeriod;
    }

    public string ConnectionString { get; }

    public TimeSpan RetentionPeriod { get; }

    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS audit_entries (
                id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                occurred_at timestamptz NOT NULL
            );

            DELETE FROM audit_entries
            WHERE occurred_at < (NOW() AT TIME ZONE 'UTC') - INTERVAL '@days day';
        ";
        // Parameterize interval days
        var days = (int)Math.Floor(RetentionPeriod.TotalDays);
        cmd.CommandText = cmd.CommandText.Replace("@days", days.ToString());

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
