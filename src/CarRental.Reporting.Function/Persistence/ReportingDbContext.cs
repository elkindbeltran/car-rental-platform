using Microsoft.EntityFrameworkCore;

namespace CarRental.Reporting.Worker.Persistence;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<ReportSubscription> ReportSubscriptions => Set<ReportSubscription>();
    public DbSet<ReportMetadata> Reports => Set<ReportMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var subscription = modelBuilder.Entity<ReportSubscription>();
        subscription.ToTable("ReportSubscriptions", "reporting");
        subscription.HasKey(entity => entity.Id);
        subscription.Property(entity => entity.OwnerUserId).HasMaxLength(256);
        subscription.Property(entity => entity.RecipientEmail).HasMaxLength(320);
        subscription.Property(entity => entity.ReportType).HasMaxLength(100);

        var report = modelBuilder.Entity<ReportMetadata>();
        report.ToTable("Reports", "reporting");
        report.HasKey(entity => entity.Id);
        report.Property(entity => entity.OwnerUserId).HasMaxLength(256);
        report.Property(entity => entity.RecipientEmail).HasMaxLength(320);
        report.Property(entity => entity.ReportType).HasMaxLength(100);
        report.Property(entity => entity.BlobContainer).HasMaxLength(63);
        report.Property(entity => entity.BlobName).HasMaxLength(1_024);
        report.Property(entity => entity.RowVersion).IsRowVersion();
        report.HasIndex(entity => new { entity.SubscriptionId, entity.PeriodStartUtc, entity.PeriodEndUtc }).IsUnique();
    }
}
