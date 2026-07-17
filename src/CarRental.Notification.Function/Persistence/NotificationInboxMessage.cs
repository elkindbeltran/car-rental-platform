namespace CarRental.Notification.Worker.Persistence;

internal sealed class NotificationInboxMessage
{
    public string MessageId { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public NotificationInboxStatus Status { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? LockedUntilUtc { get; set; }
    public string? LastFailure { get; set; }
}
