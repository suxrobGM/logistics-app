using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class RecurringDeductionEntityConfiguration : IEntityTypeConfiguration<RecurringDeduction>
{
    public void Configure(EntityTypeBuilder<RecurringDeduction> builder)
    {
        builder.ToTable("recurring_deductions");

        builder.Property(d => d.Description).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Amount).HasPrecision(18, 2);
        builder.Property(d => d.EscrowTarget).HasPrecision(18, 2);

        builder.HasOne(d => d.Employee)
            .WithMany(e => e.RecurringDeductions)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
