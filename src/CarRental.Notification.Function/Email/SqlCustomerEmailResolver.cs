using CarRental.Notification.Worker.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Notification.Worker.Email;

internal sealed class SqlCustomerEmailResolver(NotificationInboxDbContext dbContext) : ICustomerEmailResolver
{
    public Task<string?> ResolveAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        dbContext.Database
            .SqlQuery<string>($"SELECT [Email] AS [Value] FROM [dbo].[Customers] WHERE [Id] = {customerId} AND [IsDeleted] = 0")
            .SingleOrDefaultAsync(cancellationToken);
}
