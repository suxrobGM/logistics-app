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

        builder.ComplexProperty(i => i.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
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
    }
}
