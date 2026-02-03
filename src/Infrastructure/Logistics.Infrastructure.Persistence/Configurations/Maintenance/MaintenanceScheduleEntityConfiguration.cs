using Logistics.Domain.Entities.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceScheduleEntityConfiguration : IEntityTypeConfiguration<MaintenanceSchedule>
{
    public void Configure(EntityTypeBuilder<MaintenanceSchedule> builder)
    {
        builder.ToTable("maintenance_schedules");

        builder.HasIndex(i => new { i.TruckId, i.MaintenanceType });
        builder.HasIndex(i => i.NextDueDate);
        builder.HasIndex(i => i.IsActive);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);
    }
}
