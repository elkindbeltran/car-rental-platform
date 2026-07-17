using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CarRental.Application.Reporting.IntegrationEvents;

namespace CarRental.Reporting.Worker.Messaging;

public sealed class ReportGeneratedEventPublisher(ServiceBusSender sender)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync(
        ReportGeneratedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(integrationEvent, SerializerOptions))
        {
            MessageId = integrationEvent.EventId.ToString("N"),
            Subject = nameof(ReportGeneratedIntegrationEvent),
            ContentType = "application/json",
            CorrelationId = Activity.Current?.TraceId.ToString()
        };
        message.ApplicationProperties["event-type"] = nameof(ReportGeneratedIntegrationEvent);
        message.ApplicationProperties["contract-version"] = integrationEvent.ContractVersion;
        return sender.SendMessageAsync(message, cancellationToken);
    }
}
