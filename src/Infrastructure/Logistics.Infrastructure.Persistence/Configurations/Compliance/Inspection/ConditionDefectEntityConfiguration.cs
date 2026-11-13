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
            .HasMaxLength(64);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.Severity)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(d => d.Zone)
            .HasMaxLength(32);

        builder.Property(d => d.DamageType)
            .HasMaxLength(16);

        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(d => d.PhotoDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => d.LoadConditionReportId);
        builder.HasIndex(d => d.PartCategory);
    }
}
