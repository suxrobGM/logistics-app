using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class EldDriverMappingEntityConfiguration : IEntityTypeConfiguration<EldDriverMapping>
{
    public void Configure(EntityTypeBuilder<EldDriverMapping> builder)
    {
        builder.ToTable("EldDriverMappings");

        builder.HasIndex(i => new { i.ProviderType, i.ExternalDriverId })
            .IsUnique();

        builder.HasIndex(i => new { i.ProviderType, i.EmployeeId })
            .IsUnique();

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.ExternalDriverId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.ExternalDriverName)
            .HasMaxLength(200);
    }
}
