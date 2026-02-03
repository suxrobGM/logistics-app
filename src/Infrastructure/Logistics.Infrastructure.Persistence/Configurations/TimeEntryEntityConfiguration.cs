using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TimeEntryEntityConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("time_entries");

        builder.Property(t => t.TotalHours)
            .HasPrecision(10, 2);

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.HasOne(t => t.Employee)
            .WithMany(e => e.TimeEntries)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.PayrollInvoice)
            .WithMany(p => p.TimeEntries)
            .HasForeignKey(t => t.PayrollInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.EmployeeId, t.Date });
    }
}
