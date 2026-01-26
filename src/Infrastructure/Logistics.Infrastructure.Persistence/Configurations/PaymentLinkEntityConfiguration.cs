using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class PaymentLinkEntityConfiguration : IEntityTypeConfiguration<PaymentLink>
{
    public void Configure(EntityTypeBuilder<PaymentLink> builder)
    {
        builder.ToTable("PaymentLinks");

        builder.Property(p => p.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(p => p.Token).IsUnique();
        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.ExpiresAt);

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.PaymentLinks)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
