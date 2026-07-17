namespace CarRental.Application.Booking.CreateBooking;

public sealed record BookingResponse(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    DateTimeOffset PickupAtUtc,
    DateTimeOffset ReturnAtUtc,
    decimal DailyRate,
    decimal TotalAmount,
    string Currency,
    string Status,
    byte[] RowVersion);
