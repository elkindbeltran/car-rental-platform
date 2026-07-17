using Microsoft.EntityFrameworkCore;

namespace CarRental.Reporting.Worker.Persistence;

public sealed class ReportingRepository(ReportingDbContext dbContext)
{
    public Task<List<ReportSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken) =>
        dbContext.ReportSubscriptions.AsNoTracking().Where(subscription => subscription.IsActive).ToListAsync(cancellationToken);

    public Task<ReportMetadata?> FindReportAsync(
        Guid subscriptionId,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndUtc,
        CancellationToken cancellationToken) =>
        dbContext.Reports.SingleOrDefaultAsync(
            report => report.SubscriptionId == subscriptionId &&
                      report.PeriodStartUtc == periodStartUtc &&
                      report.PeriodEndUtc == periodEndUtc,
            cancellationToken);

    public Task<List<RentalReportRow>> GetRentalsAsync(
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndUtc,
        Guid? customerId,
        CancellationToken cancellationToken) =>
        dbContext.Database.SqlQuery<RentalReportRow>($$"""
            SELECT [Id] AS [BookingId], [CustomerId], [VehicleId], [PickupAtUtc], [ReturnAtUtc],
                   [TotalAmount], [Currency], CONVERT(nvarchar(30), [Status]) AS [Status]
            FROM [dbo].[Bookings]
            WHERE [CreatedAtUtc] >= {{periodStartUtc}} AND [CreatedAtUtc] < {{periodEndUtc}}
              AND ({{customerId}} IS NULL OR [CustomerId] = {{customerId}})
            ORDER BY [CreatedAtUtc]
            """).ToListAsync(cancellationToken);

    public async Task AddAsync(ReportMetadata report, CancellationToken cancellationToken)
    {
        dbContext.Reports.Add(report);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
