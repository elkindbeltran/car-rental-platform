namespace CarRental.SharedKernel.Application;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? GivenName { get; }
    string? FamilyName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
