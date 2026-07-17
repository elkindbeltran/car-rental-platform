namespace CarRental.Notification.Worker.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyClaimResult> TryBeginAsync(
        string messageId,
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task MarkCompletedAsync(string messageId, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(string messageId, string failure, CancellationToken cancellationToken = default);
}
