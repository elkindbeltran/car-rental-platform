using CarRental.SharedKernel.Application;

namespace CarRental.Domain.Booking;

public interface IBookingRepository : IRepository<Booking, Guid>
{
    Task<bool> HasOverlappingBookingAsync(
        Guid vehicleId,
        DateTimeOffset pickupAtUtc,
        DateTimeOffset returnAtUtc,
        CancellationToken cancellationToken = default);
}
