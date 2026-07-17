using CarRental.SharedKernel.Domain;

namespace CarRental.SharedKernel.Application;

public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    void Add(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}
