using System.Data;
using CarRental.Notification.Worker.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Notification.Worker.Idempotency;

internal sealed class SqlIdempotencyStore(
    NotificationInboxDbContext dbContext,
    TimeProvider timeProvider) : IIdempotencyStore
{
    private static readonly TimeSpan ProcessingLease = TimeSpan.FromMinutes(5);

    public async Task<IdempotencyClaimResult> TryBeginAsync(
        string messageId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var now = timeProvider.GetUtcNow();
            var inboxMessage = await dbContext.InboxMessages
                .SingleOrDefaultAsync(message => message.MessageId == messageId, cancellationToken);

            if (inboxMessage?.Status == NotificationInboxStatus.Completed)
            {
                await transaction.CommitAsync(cancellationToken);
                return IdempotencyClaimResult.AlreadyCompleted;
            }

            if (inboxMessage?.Status == NotificationInboxStatus.Processing && inboxMessage.LockedUntilUtc > now)
            {
                await transaction.CommitAsync(cancellationToken);
                return IdempotencyClaimResult.Busy;
            }

            if (inboxMessage is null)
            {
                inboxMessage = new NotificationInboxMessage
                {
                    MessageId = messageId,
                    EventId = eventId,
                    CreatedAtUtc = now
                };
                dbContext.InboxMessages.Add(inboxMessage);
            }

            inboxMessage.Status = NotificationInboxStatus.Processing;
            inboxMessage.Attempts++;
            inboxMessage.UpdatedAtUtc = now;
            inboxMessage.LockedUntilUtc = now.Add(ProcessingLease);
            inboxMessage.LastFailure = null;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return IdempotencyClaimResult.Acquired;
        });
    }

    public Task MarkCompletedAsync(string messageId, CancellationToken cancellationToken = default) =>
        UpdateAsync(messageId, NotificationInboxStatus.Completed, null, cancellationToken);

    public Task MarkFailedAsync(
        string messageId,
        string failure,
        CancellationToken cancellationToken = default) =>
        UpdateAsync(messageId, NotificationInboxStatus.Failed, failure, cancellationToken);

    private async Task UpdateAsync(
        string messageId,
        NotificationInboxStatus status,
        string? failure,
        CancellationToken cancellationToken)
    {
        var inboxMessage = await dbContext.InboxMessages
            .SingleAsync(message => message.MessageId == messageId, cancellationToken);
        inboxMessage.Status = status;
        inboxMessage.UpdatedAtUtc = timeProvider.GetUtcNow();
        inboxMessage.LockedUntilUtc = null;
        inboxMessage.LastFailure = failure is null ? null : failure[..Math.Min(failure.Length, 2_000)];
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
