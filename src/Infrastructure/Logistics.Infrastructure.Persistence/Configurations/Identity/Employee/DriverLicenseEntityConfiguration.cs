using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DriverLicenseEntityConfiguration : IEntityTypeConfiguration<DriverLicense>
{
    public void Configure(EntityTypeBuilder<DriverLicense> builder)
    {
        builder.ToTable("driver_licenses");

        builder.Property(i => i.LicenseNumber)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(i => i.IssuingCountry)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(i => i.IssuingRegion)
            .HasMaxLength(64);

        builder.Property(i => i.LicenseClass)
            .IsRequired();

        builder.Property(i => i.Endorsements)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasDefaultValue(DriverLicenseStatus.Active);

        builder.HasOne(i => i.Employee)
            .WithMany(e => e.Licenses)
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Document)
            .WithMany()
            .HasForeignKey(i => i.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => new { i.EmployeeId, i.LicenseNumber, i.IssuingCountry })
            .IsUnique();

        builder.HasIndex(i => i.ExpiresAt);
    }
}
