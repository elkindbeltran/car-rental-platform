namespace CarRental.Application.Booking.Abstractions;

public interface ICustomerBookingReader
{
    Task<bool> ExistsAsync(Guid customerId, CancellationToken cancellationToken = default);
}
