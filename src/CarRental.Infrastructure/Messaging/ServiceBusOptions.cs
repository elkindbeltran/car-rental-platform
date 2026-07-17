using System.ComponentModel.DataAnnotations;

namespace CarRental.Infrastructure.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    [Required]
    public string FullyQualifiedNamespace { get; init; } = string.Empty;

    [Required]
    public string TopicName { get; init; } = string.Empty;

    [Range(1, 1_440)]
    public int MessageTimeToLiveMinutes { get; init; } = 1_440;
}
