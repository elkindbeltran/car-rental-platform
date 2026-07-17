namespace CarRental.SharedKernel.Domain;

public abstract class Entity<TId> where TId : notnull
{
    protected Entity(TId id) => Id = id;

    protected Entity() { }

    public TId Id { get; protected init; } = default!;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other &&
        GetType() == other.GetType() &&
        EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
