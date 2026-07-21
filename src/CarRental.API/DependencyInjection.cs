using CarRental.API.Authentication;
using CarRental.API.ErrorHandling;
using CarRental.API.Options;
using CarRental.SharedKernel.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CarRental.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUser>();

        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection(Auth0Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var auth0 = configuration.GetRequiredSection(Auth0Options.SectionName).Get<Auth0Options>()
            ?? throw new InvalidOperationException("Auth0 configuration is missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = auth0.Authority;
                options.Audience = auth0.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = auth0.NameClaimType,
                    RoleClaimType = auth0.RoleClaimType,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole("Administrator"))
            .AddPolicy(AuthorizationPolicies.ReportDownload, policy => policy.RequireAuthenticatedUser());

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Description = "Auth0 Authorization Code flow with PKCE",
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(
                            $"{new Uri(new Uri(auth0.Authority), "authorize")}?audience={Uri.EscapeDataString(auth0.Audience)}"),
                        TokenUrl = new Uri(new Uri(auth0.Authority), "oauth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "Authenticate the user",
                            ["profile"] = "Read the user's basic profile"
                        }
                    }
                },
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            };

            options.AddSecurityDefinition("oauth2", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [securityScheme] = ["openid", "profile"]
            });
        });

        return services;
    }
}
