namespace CarRental.SharedKernel.Domain;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent() : this(DateTimeOffset.UtcNow) { }

    protected DomainEvent(DateTimeOffset occurredOnUtc) => OccurredOnUtc = occurredOnUtc;

    public DateTimeOffset OccurredOnUtc { get; }
}
