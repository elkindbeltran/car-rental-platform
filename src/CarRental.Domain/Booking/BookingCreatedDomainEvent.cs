using CarRental.SharedKernel.Domain;

namespace CarRental.Domain.Booking;

public sealed record BookingCreatedDomainEvent : DomainEvent
{
    public BookingCreatedDomainEvent(
        Guid bookingId,
        Guid customerId,
        Guid vehicleId,
        DateTimeOffset pickupAtUtc,
        DateTimeOffset returnAtUtc,
        DateTimeOffset occurredOnUtc) : base(occurredOnUtc)
    {
        BookingId = bookingId;
        CustomerId = customerId;
        VehicleId = vehicleId;
        PickupAtUtc = pickupAtUtc;
        ReturnAtUtc = returnAtUtc;
    }

    public Guid BookingId { get; }
    public Guid CustomerId { get; }
    public Guid VehicleId { get; }
    public DateTimeOffset PickupAtUtc { get; }
    public DateTimeOffset ReturnAtUtc { get; }
}
