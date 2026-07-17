using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomerEntity = CarRental.Domain.Customer.Customer;

namespace CarRental.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : ConcurrencyConfiguration<CustomerEntity>
{
    public override void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("Customers", "customer");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(CustomerEntity.MaximumNameLength).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(CustomerEntity.MaximumNameLength).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(CustomerEntity.MaximumEmailLength).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(CustomerEntity.MaximumPhoneLength);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(256);
        builder.Property(x => x.DeletedBy).HasMaxLength(256);
        builder.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Ignore(x => x.DomainEvents);
    }
}
