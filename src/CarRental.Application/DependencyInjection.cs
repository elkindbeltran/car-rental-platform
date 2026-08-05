using CarRental.Application.Behaviors;
using CarRental.Application.Abstractions.Messaging;
using CarRental.Application.Messaging;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(configuration => { }, assembly);
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IApplicationMediator, ApplicationMediator>();

        foreach (var implementationType in assembly.GetTypes().Where(type => type is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var serviceType in implementationType.GetInterfaces()
                         .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IMessageHandler<,>)))
            {
                services.AddTransient(serviceType, implementationType);
            }
        }

        return services;
    }
}
