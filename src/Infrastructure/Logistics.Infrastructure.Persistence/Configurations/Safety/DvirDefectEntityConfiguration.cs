using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DvirDefectEntityConfiguration : IEntityTypeConfiguration<DvirDefect>
{
    public void Configure(EntityTypeBuilder<DvirDefect> builder)
    {
        builder.ToTable("dvir_defects");

        builder.HasIndex(i => i.DvirReportId);
        builder.HasIndex(i => new { i.Category, i.Severity });

        builder.Property(i => i.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.CorrectionNotes)
            .HasMaxLength(1000);

        builder.HasOne(i => i.CorrectedBy)
            .WithMany()
            .HasForeignKey(i => i.CorrectedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
