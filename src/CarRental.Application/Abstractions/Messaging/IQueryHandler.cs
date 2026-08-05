using MediatR;

namespace CarRental.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>, IMessageHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
