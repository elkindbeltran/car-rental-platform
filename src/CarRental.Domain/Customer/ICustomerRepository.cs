using CarRental.SharedKernel.Application;

namespace CarRental.Domain.Customer;

public interface ICustomerRepository : IRepository<Customer, Guid>
{
    Task<bool> EmailExistsAsync(string email, Guid? excludingCustomerId, CancellationToken cancellationToken = default);
    void SetOriginalRowVersion(Customer customer, byte[] rowVersion);
}
