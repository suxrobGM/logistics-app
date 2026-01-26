using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DriverHosStatusEntityConfiguration : IEntityTypeConfiguration<DriverHosStatus>
{
    public void Configure(EntityTypeBuilder<DriverHosStatus> builder)
    {
        builder.ToTable("DriverHosStatuses");

        builder.HasIndex(i => i.EmployeeId)
            .IsUnique();

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.ExternalDriverId)
            .HasMaxLength(100);
    }
}
