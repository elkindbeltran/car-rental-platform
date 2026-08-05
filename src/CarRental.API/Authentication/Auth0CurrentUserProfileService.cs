using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CarRental.SharedKernel.Application;

namespace CarRental.API.Authentication;

internal sealed class Auth0CurrentUserProfileService(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<Auth0CurrentUserProfileService> logger) : ICurrentUserProfileService
{
    private static readonly Action<ILogger, int, Exception?> LogUserInfoFailure =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(1001, "Auth0UserInfoFailure"),
            "Auth0 userinfo returned status code {StatusCode}");

    public async Task<CurrentUserProfile?> GetAsync(CancellationToken cancellationToken = default)
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", header.Parameter);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            LogUserInfoFailure(logger, (int)response.StatusCode, null);
            return null;
        }

        var profile = await response.Content.ReadFromJsonAsync<Auth0UserInfo>(cancellationToken);
        return string.IsNullOrWhiteSpace(profile?.Email)
            ? null
            : new CurrentUserProfile(profile.Email, profile.GivenName, profile.FamilyName);
    }

    private sealed record Auth0UserInfo(
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("given_name")] string? GivenName,
        [property: JsonPropertyName("family_name")] string? FamilyName);
}
