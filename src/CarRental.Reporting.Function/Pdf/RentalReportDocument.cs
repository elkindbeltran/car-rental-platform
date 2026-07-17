using CarRental.Reporting.Worker.Persistence;

namespace CarRental.Reporting.Worker.Pdf;

public sealed record RentalReportDocument(
    Guid ReportId,
    string ReportType,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndUtc,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<RentalReportRow> Rentals);
