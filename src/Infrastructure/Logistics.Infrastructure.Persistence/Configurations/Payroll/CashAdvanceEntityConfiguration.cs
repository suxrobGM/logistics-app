using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class CashAdvanceEntityConfiguration : IEntityTypeConfiguration<CashAdvance>
{
    public void Configure(EntityTypeBuilder<CashAdvance> builder)
    {
        builder.ToTable("cash_advances");

        builder.ComplexProperty(a => a.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(a => a.Reason).HasMaxLength(500);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting a settlement hands its advances back to be deducted again.
        builder.HasOne<PayrollInvoice>()
            .WithMany()
            .HasForeignKey(a => a.PayrollInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.EmployeeId, a.Status });
    }
}
