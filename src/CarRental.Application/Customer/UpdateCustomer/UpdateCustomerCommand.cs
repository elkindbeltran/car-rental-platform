using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.UpdateCustomer;

public sealed record UpdateCustomerCommand(Guid Id, string FirstName, string LastName, string Email, string? Phone, string RowVersion)
    : ICommand<Result<CustomerResponse>>;
