using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class IftaQuarterSnapshotEntityConfiguration : IEntityTypeConfiguration<IftaQuarterSnapshot>
{
    public void Configure(EntityTypeBuilder<IftaQuarterSnapshot> builder)
    {
        builder.ToTable("ifta_quarter_snapshots");

        builder.HasIndex(i => new { i.Year, i.Quarter })
            .IsUnique();

        builder.Property(i => i.ReportJson)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
