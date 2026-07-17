using CarRental.SharedKernel.Results;

namespace CarRental.Application.Reporting.DownloadReport;

public static class ReportDownloadErrors
{
    public static ResultError NotFound { get; } = ResultError.Create(
        "Report.NotFound",
        "The requested report was not found.");

    public static ResultError UserIdentityMissing { get; } = ResultError.Create(
        "Report.UserIdentityMissing",
        "An authenticated user identity is required to download a report.");
}
