using CarRental.Application.Abstractions.Messaging;

namespace CarRental.Application.Booking.IntegrationEvents;

public sealed record BookingCreatedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid BookingId,
    Guid CustomerId,
    Guid VehicleId,
    DateTimeOffset PickupAtUtc,
    DateTimeOffset ReturnAtUtc,
    decimal TotalAmount,
    string Currency) : IIntegrationEvent
{
    public int ContractVersion => 1;
}
