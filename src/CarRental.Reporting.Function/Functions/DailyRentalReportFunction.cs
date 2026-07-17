using CarRental.Application.Reporting.IntegrationEvents;
using CarRental.Reporting.Worker.Messaging;
using CarRental.Reporting.Worker.Pdf;
using CarRental.Reporting.Worker.Persistence;
using CarRental.Reporting.Worker.Storage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CarRental.Reporting.Worker.Functions;

public sealed class DailyRentalReportFunction(
    ReportingRepository repository,
    ReportBlobStorage blobStorage,
    ReportGeneratedEventPublisher eventPublisher,
    TimeProvider timeProvider,
    ILogger<DailyRentalReportFunction> logger)
{
    private static readonly Action<ILogger, Guid, int, Exception?> LogGenerated =
        LoggerMessage.Define<Guid, int>(
            LogLevel.Information,
            new EventId(4000, "RentalReportGenerated"),
            "Generated report {ReportId} containing {RentalCount} rentals");

    private static readonly Action<ILogger, Guid, Exception?> LogFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            new EventId(4001, "RentalReportGenerationFailed"),
            "Report generation failed for subscription {SubscriptionId}");

    [Function(nameof(DailyRentalReportFunction))]
    [FixedDelayRetry(5, "00:01:00")]
    public async Task RunAsync(
        [TimerTrigger("%ReportingSchedule%", UseMonitor = true, RunOnStartup = false)] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        _ = timer;
        var now = timeProvider.GetUtcNow();
        var periodEndUtc = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var periodStartUtc = periodEndUtc.AddDays(-1);
        var subscriptions = await repository.GetActiveSubscriptionsAsync(cancellationToken);
        var failures = new List<Exception>();

        foreach (var subscription in subscriptions)
        {
            try
            {
                await GenerateForSubscriptionAsync(
                    subscription,
                    periodStartUtc,
                    periodEndUtc,
                    now,
                    cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogFailed(logger, subscription.Id, exception);
                failures.Add(exception);
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException("One or more scheduled reports failed.", failures);
        }
    }

    private async Task GenerateForSubscriptionAsync(
        ReportSubscription subscription,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndUtc,
        DateTimeOffset generatedAtUtc,
        CancellationToken cancellationToken)
    {
        var existing = await repository.FindReportAsync(
            subscription.Id,
            periodStartUtc,
            periodEndUtc,
            cancellationToken);
        if (existing is not null)
        {
            await PublishIfRequiredAsync(existing, cancellationToken);
            return;
        }

        var rentals = await repository.GetRentalsAsync(
            periodStartUtc,
            periodEndUtc,
            subscription.CustomerId,
            cancellationToken);
        var reportId = Guid.NewGuid();
        var pdf = RentalReportPdfGenerator.Generate(new RentalReportDocument(
            reportId,
            subscription.ReportType,
            periodStartUtc,
            periodEndUtc,
            generatedAtUtc,
            rentals));
        var stored = await blobStorage.UploadAsync(reportId, periodStartUtc, pdf, cancellationToken);

        var metadata = new ReportMetadata
        {
            Id = reportId,
            SubscriptionId = subscription.Id,
            OwnerUserId = subscription.OwnerUserId,
            RecipientEmail = subscription.RecipientEmail,
            ReportType = subscription.ReportType,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            BlobContainer = stored.ContainerName,
            BlobName = stored.BlobName,
            ContentLength = stored.ContentLength,
            GeneratedAtUtc = generatedAtUtc,
            EventId = Guid.NewGuid()
        };

        try
        {
            await repository.AddAsync(metadata, cancellationToken);
        }
        catch
        {
            await blobStorage.DeleteIfExistsAsync(stored.BlobName, cancellationToken);
            throw;
        }

        await PublishIfRequiredAsync(metadata, cancellationToken);
        LogGenerated(logger, reportId, rentals.Count, null);
    }

    private async Task PublishIfRequiredAsync(ReportMetadata metadata, CancellationToken cancellationToken)
    {
        if (metadata.EventPublishedAtUtc is not null)
        {
            return;
        }

        var occurredOnUtc = timeProvider.GetUtcNow();
        await eventPublisher.PublishAsync(
            new ReportGeneratedIntegrationEvent(
                metadata.EventId,
                occurredOnUtc,
                metadata.Id,
                metadata.OwnerUserId,
                metadata.RecipientEmail,
                metadata.ReportType,
                metadata.PeriodStartUtc,
                metadata.PeriodEndUtc,
                $"/api/reports/{metadata.Id}/download"),
            cancellationToken);
        metadata.EventPublishedAtUtc = occurredOnUtc;
        await repository.SaveChangesAsync(cancellationToken);
    }
}
