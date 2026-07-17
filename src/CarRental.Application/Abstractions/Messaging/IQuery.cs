using MediatR;

namespace CarRental.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
