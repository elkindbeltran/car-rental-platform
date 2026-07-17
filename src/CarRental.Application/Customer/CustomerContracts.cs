namespace CarRental.Application.Customer;

public sealed record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    bool IsActive,
    string RowVersion);

public sealed record CustomerSummary(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive);

public sealed record CustomerPage(IReadOnlyCollection<CustomerSummary> Items, int Page, int PageSize, int TotalCount);

public interface ICustomerReadService
{
    Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerPage> GetPageAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
}
