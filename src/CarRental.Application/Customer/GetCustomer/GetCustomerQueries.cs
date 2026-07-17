using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.GetCustomer;

public sealed record GetCustomerQuery(Guid Id) : IQuery<Result<CustomerResponse>>;
public sealed record GetCustomersQuery(int Page, int PageSize, string? Search) : IQuery<CustomerPage>;

internal sealed class GetCustomerQueryHandler(ICustomerReadService readService) : IQueryHandler<GetCustomerQuery, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await readService.GetByIdAsync(request.Id, cancellationToken);
        return customer is null ? Result.Failure<CustomerResponse>(CustomerErrors.NotFound) : Result.Success(customer);
    }
}

internal sealed class GetCustomersQueryHandler(ICustomerReadService readService) : IQueryHandler<GetCustomersQuery, CustomerPage>
{
    public Task<CustomerPage> Handle(GetCustomersQuery request, CancellationToken cancellationToken) =>
        readService.GetPageAsync(request.Page, request.PageSize, request.Search, cancellationToken);
}
