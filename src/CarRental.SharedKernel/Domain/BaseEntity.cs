namespace CarRental.SharedKernel.Domain;

public abstract class BaseEntity<TId> where TId : notnull
{
    protected BaseEntity(TId id) => Id = id;

    protected BaseEntity() { }

    public TId Id { get; protected init; } = default!;

    public override bool Equals(object? obj) =>
        obj is BaseEntity<TId> other &&
        GetType() == other.GetType() &&
        EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) => !(left == right);
}
