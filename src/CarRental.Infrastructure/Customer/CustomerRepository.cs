using CarRental.Application.Booking.Abstractions;
using CarRental.Application.Customer;
using CarRental.Domain.Customer;
using CarRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Customer;

internal sealed class CustomerRepository(ApplicationDbContext dbContext)
    : ICustomerRepository, ICustomerBookingReader, ICustomerReadService
{
    public Task<Domain.Customer.Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Domain.Customer.Customer aggregate) => dbContext.Customers.Add(aggregate);
    public void Remove(Domain.Customer.Customer aggregate) => dbContext.Customers.Remove(aggregate);

    public Task<bool> EmailExistsAsync(string email, Guid? excludingCustomerId, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return dbContext.Customers.AnyAsync(x => x.Email == normalized && (!excludingCustomerId.HasValue || x.Id != excludingCustomerId.Value), cancellationToken);
    }

    public Task<Domain.Customer.Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return dbContext.Customers.SingleOrDefaultAsync(x => x.Email == normalized, cancellationToken);
    }

    public Task<Domain.Customer.Customer?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken = default) =>
        dbContext.Customers.SingleOrDefaultAsync(x => x.ExternalUserId == externalUserId, cancellationToken);

    public Task<bool> ExistsAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        dbContext.Customers.AnyAsync(x => x.Id == customerId && x.IsActive, cancellationToken);

    public async Task<Guid?> GetIdByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken = default) =>
        await dbContext.Customers.Where(x => x.ExternalUserId == externalUserId && x.IsActive)
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken);

    public void SetOriginalRowVersion(Domain.Customer.Customer customer, byte[] rowVersion) =>
        dbContext.Entry(customer).Property(x => x.RowVersion).OriginalValue = rowVersion;

    async Task<CustomerResponse?> ICustomerReadService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.Phone, x.IsActive, x.RowVersion })
            .SingleOrDefaultAsync(cancellationToken);

        return customer is null
            ? null
            : new CustomerResponse(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.IsActive, Convert.ToBase64String(customer.RowVersion));
    }

    public async Task<CustomerPage> GetPageAsync(int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        var query = dbContext.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FirstName.Contains(term) || x.LastName.Contains(term) || x.Email.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new CustomerSummary(x.Id, x.FirstName, x.LastName, x.Email, x.IsActive))
            .ToListAsync(cancellationToken);
        return new CustomerPage(items, page, pageSize, totalCount);
    }
}
