using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class HosLogEntityConfiguration : IEntityTypeConfiguration<HosLog>
{
    public void Configure(EntityTypeBuilder<HosLog> builder)
    {
        builder.ToTable("HosLogs");

        builder.HasIndex(i => new { i.EmployeeId, i.LogDate });

        builder.HasIndex(i => i.ExternalLogId);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Location)
            .HasMaxLength(500);

        builder.Property(i => i.Remark)
            .HasMaxLength(1000);

        builder.Property(i => i.ExternalLogId)
            .HasMaxLength(100);
    }
}
