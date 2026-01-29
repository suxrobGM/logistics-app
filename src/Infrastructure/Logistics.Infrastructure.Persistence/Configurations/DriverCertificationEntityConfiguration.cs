using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DriverCertificationEntityConfiguration : IEntityTypeConfiguration<DriverCertification>
{
    public void Configure(EntityTypeBuilder<DriverCertification> builder)
    {
        builder.ToTable("DriverCertifications");

        builder.HasIndex(i => new { i.EmployeeId, i.CertificationType });
        builder.HasIndex(i => i.ExpirationDate);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.VerifiedBy)
            .WithMany()
            .HasForeignKey(i => i.VerifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.CertificationNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.IssuingAuthority)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.IssuingState)
            .HasMaxLength(50);

        builder.Property(i => i.Endorsements)
            .HasMaxLength(500);

        builder.Property(i => i.Restrictions)
            .HasMaxLength(500);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);
    }
}
