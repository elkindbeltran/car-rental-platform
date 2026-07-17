using System.Globalization;
using System.Net;
using CarRental.Application.Booking.IntegrationEvents;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CarRental.Notification.Worker.Email;

internal sealed class SendGridNotificationEmailSender(
    ISendGridClient client,
    IOptions<SendGridOptions> options) : INotificationEmailSender
{
    public async Task SendBookingCreatedAsync(
        string recipientEmail,
        BookingCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var sender = new EmailAddress(options.Value.FromEmail, options.Value.FromName);
        var recipient = new EmailAddress(recipientEmail);
        var subject = $"Booking {integrationEvent.BookingId:N} confirmed";
        var plainText = string.Create(
            CultureInfo.InvariantCulture,
            $"Your booking is confirmed from {integrationEvent.PickupAtUtc:u} to {integrationEvent.ReturnAtUtc:u}. Total: {integrationEvent.TotalAmount:F2} {integrationEvent.Currency}.");
        var html = string.Create(
            CultureInfo.InvariantCulture,
            $"<p>Your booking is confirmed.</p><p>Pickup: {integrationEvent.PickupAtUtc:u}<br>Return: {integrationEvent.ReturnAtUtc:u}<br>Total: {integrationEvent.TotalAmount:F2} {integrationEvent.Currency}</p>");
        var message = MailHelper.CreateSingleEmail(sender, recipient, subject, plainText, html);
        message.AddCustomArg("event_id", integrationEvent.EventId.ToString("N"));
        message.AddCustomArg("booking_id", integrationEvent.BookingId.ToString("N"));

        var response = await client.SendEmailAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode is HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
        {
            throw new HttpRequestException(
                $"SendGrid transient failure ({(int)response.StatusCode}): {responseBody}",
                null,
                response.StatusCode);
        }

        throw new PermanentNotificationException(
            $"SendGrid rejected the email ({(int)response.StatusCode}): {responseBody}");
    }
}
