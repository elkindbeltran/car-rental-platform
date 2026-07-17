using System.ComponentModel.DataAnnotations;

namespace CarRental.Notification.Worker.Email;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string FromEmail { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = string.Empty;
}
