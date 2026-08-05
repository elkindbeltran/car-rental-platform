namespace CarRental.Application.Abstractions.Messaging;

public interface IMessage<out TResponse>;

public interface IMessageHandler<in TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    Task<TResponse> Handle(TMessage request, CancellationToken cancellationToken);
}
