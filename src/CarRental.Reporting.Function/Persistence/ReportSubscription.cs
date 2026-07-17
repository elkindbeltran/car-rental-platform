namespace CarRental.Reporting.Worker.Persistence;

public sealed class ReportSubscription
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string ReportType { get; set; } = "DailyRentalSummary";
    public Guid? CustomerId { get; set; }
    public bool IsActive { get; set; }
}
