using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class EmergencyAlertEntityConfiguration : IEntityTypeConfiguration<EmergencyAlert>
{
    public void Configure(EntityTypeBuilder<EmergencyAlert> builder)
    {
        builder.ToTable("EmergencyAlerts");

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.TriggeredAt);
        builder.HasIndex(i => i.DriverId);

        builder.HasOne(i => i.Driver)
            .WithMany()
            .HasForeignKey(i => i.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Trip)
            .WithMany()
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.AcknowledgedBy)
            .WithMany()
            .HasForeignKey(i => i.AcknowledgedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ResolvedBy)
            .WithMany()
            .HasForeignKey(i => i.ResolvedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Notifications)
            .WithOne(i => i.EmergencyAlert)
            .HasForeignKey(i => i.EmergencyAlertId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Address).HasMaxLength(500);
        builder.Property(i => i.Description).HasMaxLength(2000);
        builder.Property(i => i.ResolutionNotes).HasMaxLength(2000);
    }
}
