using CarRental.Application.Booking.IntegrationEvents;

namespace CarRental.Notification.Worker.Email;

public interface INotificationEmailSender
{
    Task SendBookingCreatedAsync(
        string recipientEmail,
        BookingCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
