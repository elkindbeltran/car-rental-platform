namespace CarRental.Notification.Worker.Idempotency;

public enum IdempotencyClaimResult
{
    Acquired = 1,
    AlreadyCompleted = 2,
    Busy = 3
}
