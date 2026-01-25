using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class InvoiceEntityConfiguration : IEntityTypeConfiguration<Invoice>
{
    // Invoice (TPH)
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasDiscriminator(i => i.Type)
            .HasValue<LoadInvoice>(InvoiceType.Load)
            .HasValue<SubscriptionInvoice>(InvoiceType.Subscription)
            .HasValue<PayrollInvoice>(InvoiceType.Payroll);

        builder.Property(i => i.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(i => i.Number)
            .IsUnique();

        builder.ComplexProperty(i => i.Total, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });
    }

    #region Derived Types Configuration

    // Fine-tune derived types for LoadInvoice, SubscriptionInvoice, and PayrollInvoice
    public sealed class LoadInvoiceEntityConfiguration : IEntityTypeConfiguration<LoadInvoice>
    {
        public void Configure(EntityTypeBuilder<LoadInvoice> builder)
        {
            builder.HasOne(i => i.Load)
                .WithOne(l => l.Invoice)
                .HasForeignKey<LoadInvoice>(i => i.LoadId);
        }
    }

    public sealed class SubscriptionInvoiceEntityConfiguration : IEntityTypeConfiguration<SubscriptionInvoice>
    {
        public void Configure(EntityTypeBuilder<SubscriptionInvoice> builder)
        {
            // builder.HasOne(i => i.Subscription)
            //     .WithMany(s => s.Invoices)
            //     .HasForeignKey(i => i.SubscriptionId);
        }
    }

    public sealed class PayrollInvoiceEntityConfiguration : IEntityTypeConfiguration<PayrollInvoice>
    {
        public void Configure(EntityTypeBuilder<PayrollInvoice> builder)
        {
            builder.HasOne(i => i.Employee)
                .WithMany(e => e.PayrollInvoices)
                .HasForeignKey(i => i.EmployeeId);

            builder.HasOne(i => i.ApprovedBy)
                .WithMany()
                .HasForeignKey(i => i.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(i => i.TotalHoursWorked)
                .HasPrecision(10, 2);

            builder.Property(i => i.ApprovalNotes)
                .HasMaxLength(1000);

            builder.Property(i => i.RejectionReason)
                .HasMaxLength(1000);
        }
    }

    #endregion
}
