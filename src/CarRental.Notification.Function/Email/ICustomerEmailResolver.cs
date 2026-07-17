namespace CarRental.Notification.Worker.Email;

public interface ICustomerEmailResolver
{
    Task<string?> ResolveAsync(Guid customerId, CancellationToken cancellationToken = default);
}
