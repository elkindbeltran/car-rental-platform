namespace CarRental.SharedKernel.Domain;

public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditableEntity
    where TId : notnull
{
    protected AuditableEntity(TId id) : base(id) { }

    protected AuditableEntity() { }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
