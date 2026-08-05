using CarRental.SharedKernel.Results;

namespace CarRental.Application.Customer;

public static class CustomerErrors
{
    public static ResultError NotFound { get; } = ResultError.Create("Customer.NotFound", "The customer was not found.");
    public static ResultError EmailInUse { get; } = ResultError.Create("Customer.EmailInUse", "The email address is already assigned to a customer.");
    public static ResultError ProfileClaimsMissing { get; } = ResultError.Create(
        "Customer.ProfileClaimsMissing", "The authenticated user's email address could not be resolved.");
    public static ResultError ProfileAlreadyLinked { get; } = ResultError.Create(
        "Customer.ProfileAlreadyLinked", "This email address is already linked to another member account.");
}
