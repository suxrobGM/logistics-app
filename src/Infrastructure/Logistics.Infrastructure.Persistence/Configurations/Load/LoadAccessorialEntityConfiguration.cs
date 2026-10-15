using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadAccessorialEntityConfiguration : IEntityTypeConfiguration<LoadAccessorial>
{
    public void Configure(EntityTypeBuilder<LoadAccessorial> builder)
    {
        builder.ToTable("load_accessorials");

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Rate).HasPrecision(18, 2);
        builder.Property(a => a.Quantity).HasPrecision(12, 2);
        builder.Ignore(a => a.Total);

        builder.HasOne(a => a.Load)
            .WithMany(l => l.Accessorials)
            .HasForeignKey(a => a.LoadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.LoadId);
    }
}
