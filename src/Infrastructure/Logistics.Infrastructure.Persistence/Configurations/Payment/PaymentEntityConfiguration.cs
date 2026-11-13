using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasIndex(p => p.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("stripe_payment_intent_id IS NOT NULL");

        builder.HasIndex(p => p.StripeTransferId)
            .IsUnique()
            .HasFilter("stripe_transfer_id IS NOT NULL");

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId);
    }
}
