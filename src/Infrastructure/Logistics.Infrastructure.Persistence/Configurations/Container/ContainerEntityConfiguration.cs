using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ContainerEntityConfiguration : IEntityTypeConfiguration<Container>
{
    public void Configure(EntityTypeBuilder<Container> builder)
    {
        builder.ToTable("containers");

        builder.Property(c => c.Number)
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(c => c.Number).IsUnique();

        builder.Property(c => c.SealNumber).HasMaxLength(50);
        builder.Property(c => c.BookingReference).HasMaxLength(100);
        builder.Property(c => c.BillOfLadingNumber).HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(2000);

        builder.Property(c => c.GrossWeight).HasPrecision(18, 2);

        builder.HasOne(c => c.CurrentTerminal)
            .WithMany()
            .HasForeignKey(c => c.CurrentTerminalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
