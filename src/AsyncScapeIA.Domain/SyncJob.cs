namespace AsyncScapeIA.Domain;

public enum SyncJobStatus
{
    Pending,
    Running,
    Succeeded,
    Failed,
    Partial,
    Cancelled
}

public enum SyncTrigger
{
    Scheduled,
    Manual,
    Api
}

public sealed class SyncJob
{
    private SyncJob(
        Guid id,
        Guid connectorConfigurationId,
        SyncTrigger triggeredBy,
        SyncJobStatus status,
        DateTimeOffset startedAtUtc,
        DateTimeOffset? completedAtUtc,
        int itemsProcessed,
        int conflictsDetected,
        string? errorSummary,
        Guid correlationId)
    {
        Id = id;
        ConnectorConfigurationId = connectorConfigurationId;
        TriggeredBy = triggeredBy;
        Status = status;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        ItemsProcessed = itemsProcessed;
        ConflictsDetected = conflictsDetected;
        ErrorSummary = errorSummary;
        CorrelationId = correlationId;
    }

    public Guid Id { get; }

    public Guid ConnectorConfigurationId { get; }

    public SyncTrigger TriggeredBy { get; }

    public SyncJobStatus Status { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public int ItemsProcessed { get; private set; }

    public int ConflictsDetected { get; private set; }

    public string? ErrorSummary { get; private set; }

    public Guid CorrelationId { get; private set; }

    public static SyncJob Start(Guid connectorConfigurationId, SyncTrigger triggeredBy, Guid? correlationId = null, DateTimeOffset? startedAtUtc = null)
    {
        if (connectorConfigurationId == Guid.Empty)
        {
            throw new DomainRuleException("Connector configuration identifier is required");
        }

        var start = startedAtUtc ?? DateTimeOffset.UtcNow;
        return new SyncJob(
            Guid.NewGuid(),
            connectorConfigurationId,
            triggeredBy,
            SyncJobStatus.Pending,
            start,
            null,
            0,
            0,
            null,
            correlationId ?? Guid.NewGuid());
    }

    public void MarkRunning()
    {
        if (Status != SyncJobStatus.Pending)
        {
            throw new DomainRuleException("Sync job can only transition to running from pending");
        }

        Status = SyncJobStatus.Running;
        StartedAtUtc = DateTimeOffset.UtcNow;
    }

    public void CompleteSuccess(int itemsProcessed)
    {
        EnsureRunning();

        if (itemsProcessed < 0)
        {
            throw new DomainRuleException("Items processed cannot be negative");
        }

        ItemsProcessed = itemsProcessed;
        ConflictsDetected = 0;
        Status = SyncJobStatus.Succeeded;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void CompletePartial(int itemsProcessed, int conflictsDetected, string? errorSummary)
    {
        EnsureRunning();

        if (itemsProcessed < 0)
        {
            throw new DomainRuleException("Items processed cannot be negative");
        }

        if (conflictsDetected <= 0)
        {
            throw new DomainRuleException("Partial completion requires conflicts detected");
        }

        ItemsProcessed = itemsProcessed;
        ConflictsDetected = conflictsDetected;
        Status = SyncJobStatus.Partial;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        ErrorSummary = string.IsNullOrWhiteSpace(errorSummary) ? "Conflicts detected during sync" : errorSummary.Trim();
    }

    public void Fail(string errorSummary, int itemsProcessed, int conflictsDetected)
    {
        EnsureRunning();

        if (string.IsNullOrWhiteSpace(errorSummary))
        {
            throw new DomainRuleException("Error summary is required for failures");
        }

        if (itemsProcessed < 0)
        {
            throw new DomainRuleException("Items processed cannot be negative");
        }

        if (conflictsDetected < 0)
        {
            throw new DomainRuleException("Conflicts detected cannot be negative");
        }

        ItemsProcessed = itemsProcessed;
        ConflictsDetected = conflictsDetected;
        Status = SyncJobStatus.Failed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        ErrorSummary = errorSummary.Trim();
    }

    public void Cancel()
    {
        if (Status is SyncJobStatus.Succeeded or SyncJobStatus.Partial or SyncJobStatus.Failed)
        {
            throw new DomainRuleException("Completed jobs cannot be cancelled");
        }

        Status = SyncJobStatus.Cancelled;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    private void EnsureRunning()
    {
        if (Status is not SyncJobStatus.Pending and not SyncJobStatus.Running)
        {
            throw new DomainRuleException("Sync job must be running");
        }

        if (Status == SyncJobStatus.Pending)
        {
            MarkRunning();
        }
    }
}
