using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class NegotiationMessageEntityConfiguration : IEntityTypeConfiguration<NegotiationMessage>
{
    public void Configure(EntityTypeBuilder<NegotiationMessage> builder)
    {
        builder.ToTable("negotiation_messages");

        builder.HasIndex(m => new { m.NegotiationId, m.Sequence })
            .IsUnique();

        builder.Property(m => m.Subject)
            .HasMaxLength(500);

        builder.Property(m => m.TextBody)
            .HasMaxLength(32000)
            .IsRequired();

        builder.Property(m => m.RawBody)
            .HasMaxLength(65536);

        builder.Property(m => m.ProviderMessageId)
            .HasMaxLength(200);

        builder.Property(m => m.InReplyToMessageId)
            .HasMaxLength(200);

        builder.Property(m => m.ProposedRatePerMile)
            .HasPrecision(18, 2);

        builder.ComplexProperty(m => m.ProposedTotalRate, money =>
        {
            money.Property(v => v.Amount).HasPrecision(18, 2);
            money.Property(v => v.Currency).HasMaxLength(3);
        });
    }
}
