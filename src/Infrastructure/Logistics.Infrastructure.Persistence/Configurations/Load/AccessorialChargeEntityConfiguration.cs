using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AccessorialChargeEntityConfiguration : IEntityTypeConfiguration<AccessorialCharge>
{
    public void Configure(EntityTypeBuilder<AccessorialCharge> builder)
    {
        builder.ToTable("accessorial_charges");

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(a => a.Name).IsUnique();
        builder.Property(a => a.DefaultAmount).HasPrecision(18, 2);
    }
}
