namespace CarRental.Application.Booking.Abstractions;

public interface IVehicleBookingReader
{
    Task<bool> IsRentableAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
