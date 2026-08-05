namespace CarRental.SharedKernel.Application;

public sealed record CurrentUserProfile(string Email, string? GivenName, string? FamilyName);

public interface ICurrentUserProfileService
{
    Task<CurrentUserProfile?> GetAsync(CancellationToken cancellationToken = default);
}
