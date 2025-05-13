using Logistics.Domain.Entities;
using Logistics.Shared.Consts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

internal sealed class InvoiceEntityConfiguration : IEntityTypeConfiguration<Invoice>
{
    // Invoice (TPH)
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices")
            .HasDiscriminator(i => i.Type)
            .HasValue<LoadInvoice>(InvoiceType.Load)
            .HasValue<SubscriptionInvoice>(InvoiceType.Subscription)
            .HasValue<PayrollInvoice>(InvoiceType.Payroll);
        
        builder.Property(i => i.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired();

        builder.HasIndex(i => i.Number)
            .IsUnique();
        
        builder.ComplexProperty(i => i.Total, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });
    }
    
    // Fine-tune derived types for LoadInvoice, SubscriptionInvoice, and PayrollInvoice
    public sealed class LoadInvoiceEntityConfiguration : IEntityTypeConfiguration<LoadInvoice>
    {
        public void Configure(EntityTypeBuilder<LoadInvoice> builder)
        {
            builder.HasOne(i => i.Load)
                .WithMany(l => l.Invoices)
                .HasForeignKey(i => i.LoadId);
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
        }
    }
}

