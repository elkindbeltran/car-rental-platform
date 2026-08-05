using System.Security.Claims;
using CarRental.SharedKernel.Application;

namespace CarRental.API.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             Principal?.FindFirstValue("sub");

    public string? Email => ClaimValue(ClaimTypes.Email, "email", "https://car-rental.example.com/email");
    public string? GivenName => ClaimValue(ClaimTypes.GivenName, "given_name", "https://car-rental.example.com/given_name");
    public string? FamilyName => ClaimValue(ClaimTypes.Surname, "family_name", "https://car-rental.example.com/family_name");

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;

    private string? ClaimValue(params string[] types) =>
        types.Select(type => Principal?.FindFirstValue(type)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
