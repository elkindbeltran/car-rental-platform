using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Reporting.DownloadReport;

public sealed record DownloadReportQuery(Guid ReportId) : IQuery<Result<ReportDownloadLink>>;
