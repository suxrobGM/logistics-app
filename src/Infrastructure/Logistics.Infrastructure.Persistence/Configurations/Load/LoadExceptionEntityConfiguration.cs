using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadExceptionEntityConfiguration : IEntityTypeConfiguration<LoadException>
{
    public void Configure(EntityTypeBuilder<LoadException> builder)
    {
        builder.ToTable("load_exceptions");

        builder.Property(e => e.Reason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Resolution)
            .HasMaxLength(1000);

        builder.Property(e => e.ReportedByName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(e => e.Load)
            .WithMany(l => l.Exceptions)
            .HasForeignKey(e => e.LoadId)
            .OnDelete(DeleteBehavior.Cascade);

        // No FK constraint on ReportedById - users may exist in master DB only
        builder.HasIndex(e => e.LoadId);
        builder.HasIndex(e => e.ResolvedAt);
    }
}
