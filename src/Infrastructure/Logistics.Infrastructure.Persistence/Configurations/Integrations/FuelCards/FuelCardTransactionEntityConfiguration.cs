using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class FuelCardTransactionEntityConfiguration : IEntityTypeConfiguration<FuelCardTransaction>
{
    public void Configure(EntityTypeBuilder<FuelCardTransaction> builder)
    {
        builder.ToTable("fuel_card_transactions");

        // Sync idempotency key
        builder.HasIndex(i => new { i.ProviderType, i.ExternalTransactionId })
            .IsUnique();

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => new { i.TruckId, i.TransactionDate });

        builder.Property(i => i.ExternalTransactionId)
            .HasMaxLength(100)
            .IsRequired();

        // Card money can arrive in any currency, so it keeps one; plain names, not amount_amount.
        builder.ComplexProperty(i => i.Amount, amount =>
        {
            amount.Property(m => m.Amount).HasColumnName("amount");
            amount.Property(m => m.Currency).HasColumnName("currency");
        });

        builder.Property(i => i.Quantity).HasPrecision(18, 3);
        builder.Property(i => i.PricePerUnit).HasPrecision(18, 4);
        builder.Property(i => i.ProductCategory).HasMaxLength(50);
        builder.Property(i => i.MerchantName).HasMaxLength(200);
        builder.Property(i => i.MerchantCity).HasMaxLength(100);
        builder.Property(i => i.CardNumberMasked).HasMaxLength(30);
        builder.Property(i => i.ExternalCardId).HasMaxLength(100);
        builder.Property(i => i.UnitNumber).HasMaxLength(50);
        builder.Property(i => i.DriverName).HasMaxLength(150);

        builder.Property(i => i.RawPayloadJson)
            .HasColumnType("jsonb");

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.SetNull);

        // Deleting a settlement hands its fuel charges back to be deducted again.
        builder.HasOne<PayrollInvoice>()
            .WithMany()
            .HasForeignKey(i => i.PayrollInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Expense>()
            .WithMany()
            .HasForeignKey(i => i.ExpenseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
