using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CarRental.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarRental.Infrastructure.Messaging;

internal sealed class AzureServiceBusPublisher(
    ServiceBusSender sender,
    IOptions<ServiceBusOptions> options,
    ILogger<AzureServiceBusPublisher> logger) : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly Action<ILogger, string, string, Exception?> LogPublished =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(2000, "IntegrationEventPublished"),
            "Published integration event {EventType} with message ID {MessageId}");

    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventType = typeof(TEvent).Name;
        var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(integrationEvent, SerializerOptions))
        {
            MessageId = integrationEvent.EventId.ToString("N"),
            Subject = eventType,
            ContentType = "application/json",
            CorrelationId = Activity.Current?.TraceId.ToString(),
            TimeToLive = TimeSpan.FromMinutes(options.Value.MessageTimeToLiveMinutes)
        };

        message.ApplicationProperties["event-type"] = eventType;
        message.ApplicationProperties["contract-version"] = integrationEvent.ContractVersion;
        message.ApplicationProperties["occurred-on-utc"] = integrationEvent.OccurredOnUtc.ToString("O");

        await sender.SendMessageAsync(message, cancellationToken);
        LogPublished(logger, eventType, message.MessageId, null);
    }
}
