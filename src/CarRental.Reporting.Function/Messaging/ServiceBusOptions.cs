using System.ComponentModel.DataAnnotations;

namespace CarRental.Reporting.Worker.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    [Required]
    public string FullyQualifiedNamespace { get; init; } = string.Empty;

    [Required]
    public string TopicName { get; init; } = string.Empty;
}
