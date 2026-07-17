namespace CarRental.Application.Reporting.DownloadReport;

public interface IReportDownloadRepository
{
    Task<ReportFileDescriptor?> GetOwnedReportAsync(
        Guid reportId,
        string ownerUserId,
        CancellationToken cancellationToken);
}
