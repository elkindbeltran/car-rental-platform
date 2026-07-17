namespace CarRental.Application.Abstractions.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
    int ContractVersion { get; }
}
