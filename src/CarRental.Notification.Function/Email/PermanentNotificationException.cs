namespace CarRental.Notification.Worker.Email;

internal sealed class PermanentNotificationException(string message) : Exception(message);
