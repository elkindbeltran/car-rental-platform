using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Booking.CreateBooking;

public sealed record CreateBookingCommand(
    Guid CustomerId,
    Guid VehicleId,
    DateTimeOffset PickupAtUtc,
    DateTimeOffset ReturnAtUtc,
    decimal DailyRate,
    string Currency) : ICommand<Result<BookingResponse>>;
