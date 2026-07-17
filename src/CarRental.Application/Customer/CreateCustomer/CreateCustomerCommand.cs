using CarRental.Application.Abstractions.Messaging;
using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer.CreateCustomer;

public sealed record CreateCustomerCommand(string FirstName, string LastName, string Email, string? Phone)
    : ICommand<Result<CustomerResponse>>;
