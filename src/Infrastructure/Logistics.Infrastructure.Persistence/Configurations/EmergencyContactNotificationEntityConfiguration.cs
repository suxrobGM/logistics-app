using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class EmergencyContactNotificationEntityConfiguration : IEntityTypeConfiguration<EmergencyContactNotification>
{
    public void Configure(EntityTypeBuilder<EmergencyContactNotification> builder)
    {
        builder.ToTable("EmergencyContactNotifications");

        builder.HasIndex(i => i.EmergencyAlertId);
        builder.HasIndex(i => i.EmergencyContactId);

        builder.HasOne(i => i.EmergencyContact)
            .WithMany()
            .HasForeignKey(i => i.EmergencyContactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
