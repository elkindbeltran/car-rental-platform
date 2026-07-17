using CarRental.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRental.Infrastructure.Persistence.Configurations;

public abstract class ConcurrencyConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IConcurrencyTracked
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(entity => entity.RowVersion).IsRowVersion();
    }
}
