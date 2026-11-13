using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class HosViolationEntityConfiguration : IEntityTypeConfiguration<HosViolation>
{
    public void Configure(EntityTypeBuilder<HosViolation> builder)
    {
        builder.ToTable("hos_violations");

        builder.HasIndex(i => new { i.EmployeeId, i.ViolationDate });

        builder.HasIndex(i => new { i.ProviderType, i.ExternalViolationId })
            .IsUnique()
            .HasFilter("external_violation_id IS NOT NULL");

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.ExternalViolationId)
            .HasMaxLength(100);

        builder.Property(i => i.RuleSetCode)
            .HasMaxLength(32)
            .HasDefaultValue("FMCSA")
            .IsRequired();
    }
}
