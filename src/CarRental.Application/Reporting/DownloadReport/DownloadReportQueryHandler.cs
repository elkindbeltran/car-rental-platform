using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Reporting.DownloadReport;

internal sealed class DownloadReportQueryHandler(
    ICurrentUserService currentUser,
    IReportDownloadRepository repository,
    IReportDownloadLinkGenerator linkGenerator)
    : IQueryHandler<DownloadReportQuery, Result<ReportDownloadLink>>
{
    private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(5);

    public async Task<Result<ReportDownloadLink>> Handle(
        DownloadReportQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            return Result.Failure<ReportDownloadLink>(ReportDownloadErrors.UserIdentityMissing);
        }

        var report = await repository.GetOwnedReportAsync(
            request.ReportId,
            currentUser.UserId,
            cancellationToken);

        if (report is null)
        {
            return Result.Failure<ReportDownloadLink>(ReportDownloadErrors.NotFound);
        }

        var link = await linkGenerator.GenerateReadOnlyLinkAsync(
            report,
            LinkLifetime,
            cancellationToken);

        return Result.Success(link);
    }
}
