using CarRental.Application.Abstractions.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedValidationException = CarRental.SharedKernel.Exceptions.ValidationException;

namespace CarRental.Application.Messaging;

internal sealed class ApplicationMediator(IServiceProvider services) : IApplicationMediator
{
    public async Task<TResponse> SendAsync<TResponse>(
        IMessage<TResponse> message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var invokerType = typeof(MessageHandlerInvoker<,>).MakeGenericType(message.GetType(), typeof(TResponse));
        var invoker = (IMessageHandlerInvoker<TResponse>)ActivatorUtilities.CreateInstance(services, invokerType);
        return await invoker.HandleAsync(message, cancellationToken);
    }
}

internal interface IMessageHandlerInvoker<TResponse>
{
    Task<TResponse> HandleAsync(object message, CancellationToken cancellationToken);
}

internal sealed class MessageHandlerInvoker<TMessage, TResponse>(
    IMessageHandler<TMessage, TResponse> handler,
    IEnumerable<IValidator<TMessage>> validators) : IMessageHandlerInvoker<TResponse>
    where TMessage : IMessage<TResponse>
{
    public async Task<TResponse> HandleAsync(object message, CancellationToken cancellationToken)
    {
        var request = (TMessage)message;
        var validationContext = new ValidationContext<TMessage>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(validationContext, cancellationToken)));
        var failures = validationResults.SelectMany(result => result.Errors).ToArray();

        if (failures.Length > 0)
        {
            var errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

            throw new SharedValidationException(errors);
        }

        return await handler.Handle(request, cancellationToken);
    }
}
