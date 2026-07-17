using Microsoft.EntityFrameworkCore;

namespace CarRental.Notification.Worker.Persistence;

internal sealed class NotificationInboxDbContext(DbContextOptions<NotificationInboxDbContext> options)
    : DbContext(options)
{
    public DbSet<NotificationInboxMessage> InboxMessages => Set<NotificationInboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var inbox = modelBuilder.Entity<NotificationInboxMessage>();
        inbox.ToTable("NotificationInbox", "worker");
        inbox.HasKey(message => message.MessageId);
        inbox.Property(message => message.MessageId).HasMaxLength(128);
        inbox.Property(message => message.Status).HasConversion<string>().HasMaxLength(20);
        inbox.Property(message => message.LastFailure).HasMaxLength(2_000);
        inbox.HasIndex(message => new { message.Status, message.UpdatedAtUtc });
    }
}
