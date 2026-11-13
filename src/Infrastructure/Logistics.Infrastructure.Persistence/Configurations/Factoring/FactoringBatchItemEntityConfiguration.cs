using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class FactoringBatchItemEntityConfiguration : IEntityTypeConfiguration<FactoringBatchItem>
{
    public void Configure(EntityTypeBuilder<FactoringBatchItem> builder)
    {
        builder.ToTable("factoring_batch_items");

        builder.Property(i => i.RejectionReason).HasMaxLength(1000);
        builder.Ignore(i => i.ReserveOutstanding);

        builder.HasOne(i => i.Invoice)
            .WithMany()
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.InvoiceId);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(i => i.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
