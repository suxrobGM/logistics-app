using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class NotificationReadEntityConfiguration : IEntityTypeConfiguration<NotificationRead>
{
    public void Configure(EntityTypeBuilder<NotificationRead> builder)
    {
        builder.ToTable("notification_reads");

        // MarkNotificationsReadHandler inserts against this index with ON CONFLICT DO NOTHING.
        builder.HasIndex(r => new { r.NotificationId, r.UserId })
            .IsUnique();

        builder.HasOne<Notification>()
            .WithMany()
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
