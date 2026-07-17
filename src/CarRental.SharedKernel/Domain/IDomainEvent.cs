namespace CarRental.SharedKernel.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
