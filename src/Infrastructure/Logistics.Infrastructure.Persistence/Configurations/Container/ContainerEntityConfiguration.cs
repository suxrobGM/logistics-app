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

        builder.Property(c => c.SteamshipLineScac).HasMaxLength(4);
        builder.Property(c => c.SteamshipLineName).HasMaxLength(100);
        builder.Property(c => c.PickupAppointmentNumber).HasMaxLength(50);
        builder.Property(c => c.ReturnAppointmentNumber).HasMaxLength(50);
        builder.Property(c => c.ChassisNumber).HasMaxLength(20);
        builder.Property(c => c.StreetTurnBookingReference).HasMaxLength(100);

        builder.Property(c => c.DemurrageRatePerDay).HasPrecision(18, 2);
        builder.Property(c => c.PerDiemRatePerDay).HasPrecision(18, 2);
        builder.Property(c => c.ChassisRatePerDay).HasPrecision(18, 2);

        builder.HasOne(c => c.EmptyReturnTerminal)
            .WithMany()
            .HasForeignKey(c => c.EmptyReturnTerminalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
