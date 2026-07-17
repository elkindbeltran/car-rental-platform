namespace CarRental.Application.Reporting.DownloadReport;

public interface IReportDownloadLinkGenerator
{
    Task<ReportDownloadLink> GenerateReadOnlyLinkAsync(
        ReportFileDescriptor report,
        TimeSpan lifetime,
        CancellationToken cancellationToken);
}
