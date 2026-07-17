using CarRental.Application.Abstractions.Messaging;

namespace CarRental.Application.Reporting.IntegrationEvents;

public sealed record ReportGeneratedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid ReportId,
    string OwnerUserId,
    string RecipientEmail,
    string ReportType,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndUtc,
    string DownloadPath) : IIntegrationEvent
{
    public int ContractVersion => 1;
}
