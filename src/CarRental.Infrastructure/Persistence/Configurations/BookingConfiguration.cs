using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingEntity = CarRental.Domain.Booking.Booking;

namespace CarRental.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : ConcurrencyConfiguration<BookingEntity>
{
    public override void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("Bookings", "booking");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DailyRate).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(256);
        builder.HasIndex(x => new { x.VehicleId, x.PickupAtUtc, x.ReturnAtUtc });
        builder.HasIndex(x => x.CustomerId);
        builder.Ignore(x => x.DomainEvents);
    }
}
