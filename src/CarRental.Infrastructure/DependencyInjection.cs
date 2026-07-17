using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Application.Booking.Abstractions;
using CarRental.Application.Customer;
using CarRental.Application.Inventory;
using CarRental.Application.Reporting.DownloadReport;
using CarRental.Domain.Customer;
using CarRental.Domain.Booking;
using CarRental.Domain.Inventory;
using CarRental.Infrastructure.Booking;
using CarRental.Infrastructure.Customer;
using CarRental.Infrastructure.Inventory;
using CarRental.Infrastructure.Messaging;
using CarRental.Infrastructure.Persistence;
using CarRental.Infrastructure.Reporting;
using CarRental.Infrastructure.Time;
using CarRental.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Infrastructure;

public static class DependencyInjection
{
    private const string DatabaseConnectionName = "CarRentalDatabase";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DatabaseConnectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{DatabaseConnectionName}' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<CustomerRepository>();
        services.AddScoped<ICustomerRepository>(provider => provider.GetRequiredService<CustomerRepository>());
        services.AddScoped<ICustomerBookingReader>(provider => provider.GetRequiredService<CustomerRepository>());
        services.AddScoped<ICustomerReadService>(provider => provider.GetRequiredService<CustomerRepository>());
        services.AddScoped<VehicleRepository>();
        services.AddScoped<IVehicleRepository>(provider => provider.GetRequiredService<VehicleRepository>());
        services.AddScoped<IVehicleBookingReader>(provider => provider.GetRequiredService<VehicleRepository>());
        services.AddScoped<IVehicleReadService>(provider => provider.GetRequiredService<VehicleRepository>());
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddReportDownloads(configuration);
        services.AddAzureServiceBus(configuration);
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("azure-sql");

        return services;
    }

    private static void AddReportDownloads(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BlobStorageOptions>()
            .Bind(configuration.GetSection(BlobStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<BlobStorageOptions>>()
                .Value;

            return new BlobServiceClient(
                new Uri(options.ServiceUri),
                new DefaultAzureCredential());
        });
        services.AddScoped<IReportDownloadRepository, ReportDownloadRepository>();
        services.AddSingleton<IReportDownloadLinkGenerator, UserDelegationSasLinkGenerator>();
    }

    private static void AddAzureServiceBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ServiceBusOptions>()
            .Bind(configuration.GetSection(ServiceBusOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => options.FullyQualifiedNamespace.EndsWith(
                    ".servicebus.windows.net",
                    StringComparison.OrdinalIgnoreCase),
                "AzureServiceBus:FullyQualifiedNamespace must be an Azure Service Bus namespace.")
            .ValidateOnStart();

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions =
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 5,
                    Delay = TimeSpan.FromSeconds(0.8),
                    MaxDelay = TimeSpan.FromSeconds(30),
                    TryTimeout = TimeSpan.FromSeconds(60)
                }
            };

            return new ServiceBusClient(
                options.FullyQualifiedNamespace,
                new DefaultAzureCredential(),
                clientOptions);
        });
        services.AddSingleton(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<ServiceBusClient>();
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
            return client.CreateSender(options.TopicName);
        });
        services.AddSingleton<IIntegrationEventPublisher, AzureServiceBusPublisher>();
    }
}
