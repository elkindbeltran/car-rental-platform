namespace CarRental.Reporting.Worker.Persistence;

public sealed class ReportMetadata
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTimeOffset PeriodStartUtc { get; set; }
    public DateTimeOffset PeriodEndUtc { get; set; }
    public string BlobContainer { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public Guid EventId { get; set; }
    public DateTimeOffset? EventPublishedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
