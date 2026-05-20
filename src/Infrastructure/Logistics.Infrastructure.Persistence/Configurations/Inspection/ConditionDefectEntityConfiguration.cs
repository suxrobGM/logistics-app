using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ConditionDefectEntityConfiguration : IEntityTypeConfiguration<ConditionDefect>
{
    public void Configure(EntityTypeBuilder<ConditionDefect> builder)
    {
        builder.ToTable("condition_defects");

        builder.Property(d => d.PartCategory)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(d => d.LoadConditionReportId);
        builder.HasIndex(d => d.PartCategory);
    }
}
