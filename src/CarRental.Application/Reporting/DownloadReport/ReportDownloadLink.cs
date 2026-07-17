namespace CarRental.Application.Reporting.DownloadReport;

public sealed record ReportDownloadLink(Uri Uri, DateTimeOffset ExpiresAtUtc);
