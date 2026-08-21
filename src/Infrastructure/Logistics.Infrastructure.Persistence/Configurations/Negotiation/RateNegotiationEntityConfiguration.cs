using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class RateNegotiationEntityConfiguration : IEntityTypeConfiguration<RateNegotiation>
{
    public void Configure(EntityTypeBuilder<RateNegotiation> builder)
    {
        builder.ToTable("rate_negotiations");

        builder.HasIndex(n => n.ReplyToken)
            .IsUnique();

        // At most one live thread per listing; closed threads stay for the audit trail.
        // Status is stored as a snake_case string by SnakeCaseEnumConvention.
        builder.HasIndex(n => n.LoadBoardListingId)
            .IsUnique()
            .HasFilter("status IN ('awaiting_broker', 'broker_replied')");

        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.ExpiresAt);

        builder.Property(n => n.ReplyToken)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(n => n.BrokerEmail)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.BrokerName)
            .HasMaxLength(200);

        builder.Property(n => n.BrokerMcNumber)
            .HasMaxLength(20);

        builder.Property(n => n.CloseReason)
            .HasMaxLength(1000);

        builder.Property(n => n.FloorRatePerMile)
            .HasPrecision(18, 2);

        builder.ComplexProperty(n => n.FloorTotalRate, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.ComplexProperty(n => n.LatestCounterOffer, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.ComplexProperty(n => n.LatestBrokerOffer, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.HasOne(n => n.LoadBoardListing)
            .WithMany()
            .HasForeignKey(n => n.LoadBoardListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Load)
            .WithMany()
            .HasForeignKey(n => n.LoadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(n => n.Messages)
            .WithOne(m => m.Negotiation)
            .HasForeignKey(m => m.NegotiationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
