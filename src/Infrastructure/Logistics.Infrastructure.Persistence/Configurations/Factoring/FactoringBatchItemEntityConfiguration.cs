using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class FactoringBatchItemEntityConfiguration : IEntityTypeConfiguration<FactoringBatchItem>
{
    public void Configure(EntityTypeBuilder<FactoringBatchItem> builder)
    {
        builder.ToTable("factoring_batch_items");

        builder.ComplexProperty(i => i.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(i => i.AdvanceAmount).HasPrecision(18, 2);
        builder.Property(i => i.FeeAmount).HasPrecision(18, 2);
        builder.Property(i => i.ReserveAmount).HasPrecision(18, 2);
        builder.Property(i => i.ReserveReleasedAmount).HasPrecision(18, 2);
        builder.Property(i => i.RejectionReason).HasMaxLength(1000);
        builder.Ignore(i => i.ReserveOutstanding);

        builder.HasOne(i => i.Invoice)
            .WithMany()
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.InvoiceId);
    }
}
