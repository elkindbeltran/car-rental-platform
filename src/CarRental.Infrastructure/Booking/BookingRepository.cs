using CarRental.Domain.Booking;
using CarRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Booking;

internal sealed class BookingRepository(ApplicationDbContext dbContext) : IBookingRepository
{
    public Task<Domain.Booking.Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Bookings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Domain.Booking.Booking aggregate) => dbContext.Bookings.Add(aggregate);
    public void Remove(Domain.Booking.Booking aggregate) => dbContext.Bookings.Remove(aggregate);

    public Task<bool> HasOverlappingBookingAsync(Guid vehicleId, DateTimeOffset pickupAtUtc, DateTimeOffset returnAtUtc, CancellationToken cancellationToken = default) =>
        dbContext.Bookings.AnyAsync(
            x => x.VehicleId == vehicleId &&
                 x.Status == BookingStatus.Confirmed &&
                 x.PickupAtUtc < returnAtUtc &&
                 pickupAtUtc < x.ReturnAtUtc,
            cancellationToken);
}
