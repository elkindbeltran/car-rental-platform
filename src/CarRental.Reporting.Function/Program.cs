using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using CarRental.Reporting.Worker.Messaging;
using CarRental.Reporting.Worker.Pdf;
using CarRental.Reporting.Worker.Persistence;
using CarRental.Reporting.Worker.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CarRental.Reporting.Function")
        .WriteTo.Console(new RenderedCompactJsonFormatter()))
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("CarRentalDatabase")
            ?? throw new InvalidOperationException("Connection string 'CarRentalDatabase' is not configured.");

        services.AddOptions<BlobStorageOptions>()
            .Bind(context.Configuration.GetSection(BlobStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<ServiceBusOptions>()
            .Bind(context.Configuration.GetSection(ServiceBusOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<ReportingDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)));
        services.AddScoped<ReportingRepository>();
        services.AddScoped<ReportBlobStorage>();
        services.AddScoped<ReportGeneratedEventPublisher>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BlobStorageOptions>>().Value;
            return new BlobServiceClient(new Uri(options.ServiceUri), new DefaultAzureCredential());
        });
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
            return new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential());
        });
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
            return serviceProvider.GetRequiredService<ServiceBusClient>().CreateSender(options.TopicName);
        });
    })
    .Build();

await host.RunAsync();
