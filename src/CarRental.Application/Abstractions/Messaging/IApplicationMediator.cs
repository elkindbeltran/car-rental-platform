namespace CarRental.Application.Abstractions.Messaging;

public interface IApplicationMediator
{
    Task<TResponse> SendAsync<TResponse>(
        IMessage<TResponse> message,
        CancellationToken cancellationToken = default);
}
