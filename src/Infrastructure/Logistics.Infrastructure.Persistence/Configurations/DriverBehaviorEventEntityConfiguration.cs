using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DriverBehaviorEventEntityConfiguration : IEntityTypeConfiguration<DriverBehaviorEvent>
{
    public void Configure(EntityTypeBuilder<DriverBehaviorEvent> builder)
    {
        builder.ToTable("DriverBehaviorEvents");

        builder.HasIndex(i => new { i.EmployeeId, i.OccurredAt });
        builder.HasIndex(i => i.EventType);
        builder.HasIndex(i => i.ExternalEventId);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ReviewedBy)
            .WithMany()
            .HasForeignKey(i => i.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.Location)
            .HasMaxLength(500);

        builder.Property(i => i.ExternalEventId)
            .HasMaxLength(100);

        builder.Property(i => i.ReviewNotes)
            .HasMaxLength(1000);
    }
}
