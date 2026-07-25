namespace MohCovidInsights.Domain.Entities;

public enum SyncStatus { Running, Succeeded, Failed, Skipped }

public sealed class DatasetSyncRun
{
    private DatasetSyncRun() { }

    private DatasetSyncRun(string datasetId, DateTimeOffset startedAt)
    {
        Id = Guid.NewGuid();
        DatasetId = datasetId;
        StartedAt = startedAt;
        Status = SyncStatus.Running;
    }

    public Guid Id { get; private set; }
    public string DatasetId { get; private set; } = null!;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public SyncStatus Status { get; private set; }
    public int RowCount { get; private set; }
    public string? PayloadHash { get; private set; }
    public string? FailureReason { get; private set; }

    public static DatasetSyncRun Start(string datasetId, DateTimeOffset now) => new(datasetId, now);

    public void Succeed(int rowCount, string payloadHash, DateTimeOffset now)
    {
        Status = SyncStatus.Succeeded;
        RowCount = rowCount;
        PayloadHash = payloadHash;
        CompletedAt = now;
    }

    public void Skip(string payloadHash, DateTimeOffset now)
    {
        Status = SyncStatus.Skipped;
        PayloadHash = payloadHash;
        CompletedAt = now;
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        Status = SyncStatus.Failed;
        FailureReason = reason;
        CompletedAt = now;
    }
}