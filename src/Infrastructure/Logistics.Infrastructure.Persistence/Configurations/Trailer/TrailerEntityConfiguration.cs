using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TrailerEntityConfiguration : IEntityTypeConfiguration<Trailer>
{
    public void Configure(EntityTypeBuilder<Trailer> builder)
    {
        builder.ToTable("trailers");

        builder.Property(t => t.Number).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Number).IsUnique();

        builder.Property(t => t.Vin).HasMaxLength(17);
        builder.HasIndex(t => t.Vin).IsUnique().HasFilter("vin IS NOT NULL");
        builder.Property(t => t.LicensePlate).HasMaxLength(20);
        builder.Property(t => t.LicensePlateState).HasMaxLength(10);
        builder.Property(t => t.Make).HasMaxLength(100);
        builder.Property(t => t.Model).HasMaxLength(100);
        builder.Property(t => t.LengthMeters).HasPrecision(6, 2);
        builder.Property(t => t.Notes).HasMaxLength(2000);
    }
}
