using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TrackingLinkEntityConfiguration : IEntityTypeConfiguration<TrackingLink>
{
    public void Configure(EntityTypeBuilder<TrackingLink> builder)
    {
        builder.ToTable("TrackingLinks");

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.LoadId);
        builder.HasIndex(t => t.ExpiresAt);

        builder.HasOne(t => t.Load)
            .WithMany()
            .HasForeignKey(t => t.LoadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
