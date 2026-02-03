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

        builder.HasIndex(i => i.ExternalViolationId);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.ExternalViolationId)
            .HasMaxLength(100);
    }
}
