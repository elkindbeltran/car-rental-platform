using CarRental.Notification.Worker.Email;
using CarRental.Notification.Worker.Idempotency;
using CarRental.Notification.Worker.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid;
using Serilog;
using Serilog.Formatting.Compact;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CarRental.Notification.Function")
        .WriteTo.Console(new RenderedCompactJsonFormatter()))
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("CarRentalDatabase")
            ?? throw new InvalidOperationException("Connection string 'CarRentalDatabase' is not configured.");

        services.AddOptions<SendGridOptions>()
            .Bind(context.Configuration.GetSection(SendGridOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<NotificationInboxDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdempotencyStore, SqlIdempotencyStore>();
        services.AddScoped<ICustomerEmailResolver, SqlCustomerEmailResolver>();
        services.AddScoped<INotificationEmailSender, SendGridNotificationEmailSender>();
        services.AddSingleton<ISendGridClient>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<SendGridOptions>>()
                .Value;
            return new SendGridClient(options.ApiKey);
        });
    })
    .Build();

await host.RunAsync();
