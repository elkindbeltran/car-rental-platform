using System.ComponentModel.DataAnnotations;

namespace CarRental.API.Options;

public sealed class Auth0Options
{
    public const string SectionName = "Auth0";

    [Required, Url]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string NameClaimType { get; init; } = "sub";

    [Required]
    public string RoleClaimType { get; init; } = "https://car-rental.example.com/roles";
}
