using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadBoardListingEntityConfiguration : IEntityTypeConfiguration<LoadBoardListing>
{
    public void Configure(EntityTypeBuilder<LoadBoardListing> builder)
    {
        builder.ToTable("load_board_listings");

        builder.HasIndex(i => new { i.ExternalListingId, i.ProviderType })
            .IsUnique();

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.ExpiresAt);

        builder.Property(i => i.ExternalListingId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.RatePerMile)
            .HasPrecision(18, 2);

        builder.ComplexProperty(i => i.TotalRate, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(i => i.EquipmentType)
            .HasMaxLength(50);

        builder.Property(i => i.Commodity)
            .HasMaxLength(200);

        builder.Property(i => i.BrokerName)
            .HasMaxLength(200);

        builder.Property(i => i.BrokerPhone)
            .HasMaxLength(30);

        builder.Property(i => i.BrokerEmail)
            .HasMaxLength(200);

        builder.Property(i => i.BrokerMcNumber)
            .HasMaxLength(20);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.HasOne(i => i.Load)
            .WithMany()
            .HasForeignKey(i => i.LoadId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
