using CarRental.Application.Reporting.DownloadReport;
using CarRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Reporting;

internal sealed class ReportDownloadRepository(ApplicationDbContext dbContext)
    : IReportDownloadRepository
{
    public async Task<ReportFileDescriptor?> GetOwnedReportAsync(
        Guid reportId,
        string ownerUserId,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.Database
            .SqlQuery<ReportFileRecord>($"""
                SELECT [BlobContainer] AS [ContainerName], [BlobName]
                FROM [reporting].[Reports]
                WHERE [Id] = {reportId}
                  AND [OwnerUserId] = {ownerUserId}
                """)
            .SingleOrDefaultAsync(cancellationToken);

        return record is null
            ? null
            : new ReportFileDescriptor(record.ContainerName, record.BlobName);
    }

    private sealed class ReportFileRecord
    {
        public string ContainerName { get; init; } = string.Empty;
        public string BlobName { get; init; } = string.Empty;
    }
}
