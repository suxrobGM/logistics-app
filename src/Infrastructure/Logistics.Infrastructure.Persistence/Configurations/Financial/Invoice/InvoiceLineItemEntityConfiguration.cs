using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceLineItemEntityConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("invoice_line_items");

        builder.Property(i => i.Quantity).HasPrecision(12, 2);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.TaxRatePercent)
            .HasPrecision(7, 4);

        builder.Property(i => i.TaxCode)
            .HasMaxLength(50);

        builder.HasIndex(i => i.InvoiceId);

        builder.HasOne(i => i.Invoice)
            .WithMany(inv => inv.LineItems)
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
