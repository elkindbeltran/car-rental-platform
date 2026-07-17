using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer;

public static class CustomerErrors
{
    public static ResultError NotFound { get; } = ResultError.Create("Customer.NotFound", "The customer was not found.");
    public static ResultError EmailInUse { get; } = ResultError.Create("Customer.EmailInUse", "The email address is already assigned to a customer.");
}
