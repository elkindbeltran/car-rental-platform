using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CarRental.Application.Booking.IntegrationEvents;
using CarRental.Notification.Worker.Email;
using CarRental.Notification.Worker.Idempotency;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CarRental.Notification.Worker.Functions;

public sealed class BookingCreatedNotificationFunction(
    IIdempotencyStore idempotencyStore,
    ICustomerEmailResolver customerEmailResolver,
    INotificationEmailSender emailSender,
    ILogger<BookingCreatedNotificationFunction> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly Action<ILogger, string, Exception?> LogDuplicate =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3000, "DuplicateNotificationIgnored"),
            "Notification message {MessageId} was already completed");

    private static readonly Action<ILogger, string, Guid, Exception?> LogSent =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Information,
            new EventId(3001, "BookingNotificationSent"),
            "Sent notification for message {MessageId} and booking {BookingId}");

    private static readonly Action<ILogger, string, string, Exception?> LogDeadLettered =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(3002, "NotificationDeadLettered"),
            "Dead-lettered notification message {MessageId}: {Reason}");

    private static readonly Action<ILogger, string, Exception?> LogTransientFailure =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3003, "NotificationTransientFailure"),
            "Transient notification failure for message {MessageId}; delivery will be retried");

    [Function(nameof(BookingCreatedNotificationFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%NotificationsTopicName%",
            "%NotificationsSubscriptionName%",
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        var integrationEvent = Deserialize(message);
        if (integrationEvent is null)
        {
            await DeadLetterAsync(
                message,
                messageActions,
                "InvalidContract",
                "The message body is not a valid BookingCreated contract.",
                cancellationToken);
            return;
        }

        var claim = await idempotencyStore.TryBeginAsync(
            message.MessageId,
            integrationEvent.EventId,
            cancellationToken);

        if (claim == IdempotencyClaimResult.AlreadyCompleted)
        {
            LogDuplicate(logger, message.MessageId, null);
            await messageActions.CompleteMessageAsync(message, cancellationToken);
            return;
        }

        if (claim == IdempotencyClaimResult.Busy)
        {
            throw new InvalidOperationException(
                $"Notification message '{message.MessageId}' is already being processed.");
        }

        try
        {
            var recipientEmail = await customerEmailResolver.ResolveAsync(
                integrationEvent.CustomerId,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                const string reason = "No notification email exists for the booking customer.";
                await idempotencyStore.MarkFailedAsync(message.MessageId, reason, cancellationToken);
                await DeadLetterAsync(
                    message,
                    messageActions,
                    "RecipientNotFound",
                    reason,
                    cancellationToken);
                return;
            }

            await emailSender.SendBookingCreatedAsync(recipientEmail, integrationEvent, cancellationToken);
            await idempotencyStore.MarkCompletedAsync(message.MessageId, cancellationToken);
            await messageActions.CompleteMessageAsync(message, cancellationToken);
            LogSent(logger, message.MessageId, integrationEvent.BookingId, null);
        }
        catch (PermanentNotificationException exception)
        {
            await idempotencyStore.MarkFailedAsync(message.MessageId, exception.Message, cancellationToken);
            await DeadLetterAsync(
                message,
                messageActions,
                "PermanentSendGridFailure",
                exception.Message,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await idempotencyStore.MarkFailedAsync(message.MessageId, exception.Message, cancellationToken);
            LogTransientFailure(logger, message.MessageId, exception);
            throw;
        }
    }

    private static BookingCreatedIntegrationEvent? Deserialize(ServiceBusReceivedMessage message)
    {
        try
        {
            var integrationEvent = message.Body.ToObjectFromJson<BookingCreatedIntegrationEvent>(SerializerOptions);
            if (integrationEvent is null ||
                integrationEvent.ContractVersion != 1 ||
                integrationEvent.EventId == Guid.Empty ||
                integrationEvent.BookingId == Guid.Empty ||
                integrationEvent.CustomerId == Guid.Empty)
            {
                return null;
            }

            return integrationEvent;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task DeadLetterAsync(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        string reason,
        string description,
        CancellationToken cancellationToken)
    {
        var safeDescription = description[..Math.Min(description.Length, 4_096)];
        await messageActions.DeadLetterMessageAsync(
            message,
            null,
            reason,
            safeDescription,
            cancellationToken);
        LogDeadLettered(logger, message.MessageId, reason, null);
    }
}
