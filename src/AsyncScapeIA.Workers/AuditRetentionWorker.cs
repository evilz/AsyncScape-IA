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

    public Task RunOnceAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}