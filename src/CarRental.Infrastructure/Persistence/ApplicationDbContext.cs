using CarRental.SharedKernel.Application;
using CarRental.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUser) : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.Customer.Customer> Customers => Set<Domain.Customer.Customer>();
    public DbSet<Domain.Inventory.Vehicle> Vehicles => Set<Domain.Inventory.Vehicle>();
    public DbSet<Domain.Booking.Booking> Bookings => Set<Domain.Booking.Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        ApplySoftDeletes();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = dateTimeProvider.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = userId;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.LastModifiedAtUtc = now;
                entry.Entity.LastModifiedBy = userId;
            }
        }
    }

    private void ApplySoftDeletes()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>().Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = dateTimeProvider.UtcNow;
            entry.Entity.DeletedBy = currentUser.UserId;
        }
    }
}
