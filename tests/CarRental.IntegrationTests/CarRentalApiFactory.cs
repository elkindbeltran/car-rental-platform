using CarRental.Application.Abstractions.Messaging;
using CarRental.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CarRental.IntegrationTests;

public sealed class CarRentalApiFactory : WebApplicationFactory<Program>
{
    public const string AuthenticationScheme = "IntegrationTest";
    private readonly string _databaseName = $"car-rental-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:CarRentalDatabase"] = "Server=integration-test",
                ["Auth0:Authority"] = "https://integration-test.auth0.com/",
                ["Auth0:Audience"] = "https://integration-test.car-rental",
                ["AzureServiceBus:FullyQualifiedNamespace"] = "integration-test.servicebus.windows.net",
                ["AzureServiceBus:TopicName"] = "integration-tests",
                ["BlobStorage:ServiceUri"] = "https://integrationtest.blob.core.windows.net"
            }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IIntegrationEventPublisher>();
            services.AddSingleton<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
            services.AddDataProtection().UseEphemeralDataProtectionProvider();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AuthenticationScheme;
                    options.DefaultChallengeScheme = AuthenticationScheme;
                    options.DefaultForbidScheme = AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(AuthenticationScheme, _ => { });
        });
    }

    public HttpClient CreateAuthenticatedClient(bool administrator = false, string? userId = null, string? email = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserHeader, userId ?? Guid.NewGuid().ToString("N"));
        if (email is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.EmailHeader, email);
        }
        if (administrator)
        {
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Administrator");
        }

        return client;
    }

    private sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : IIntegrationEvent => Task.CompletedTask;
    }
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string UserHeader = "X-Test-User";
    public const string RoleHeader = "X-Test-Role";
    public const string EmailHeader = "X-Test-Email";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var userId) || string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (Request.Headers.TryGetValue(EmailHeader, out var email) && !string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
            claims.Add(new Claim(ClaimTypes.GivenName, "Test"));
            claims.Add(new Claim(ClaimTypes.Surname, "Member"));
        }
        if (Request.Headers.TryGetValue(RoleHeader, out var role) && !string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CarRentalApiFactory.AuthenticationScheme));
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, CarRentalApiFactory.AuthenticationScheme)));
    }
}
