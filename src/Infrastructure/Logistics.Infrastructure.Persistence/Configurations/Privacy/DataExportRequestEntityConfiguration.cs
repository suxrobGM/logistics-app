using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations.Privacy;

internal sealed class DataExportRequestEntityConfiguration : IEntityTypeConfiguration<DataExportRequest>
{
    public void Configure(EntityTypeBuilder<DataExportRequest> builder)
    {
        builder.ToTable("data_export_requests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.BlobContainer).HasMaxLength(128);
        builder.Property(r => r.BlobName).HasMaxLength(512);
        builder.Property(r => r.ErrorMessage).HasMaxLength(2048);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.RequestedAt);
        builder.HasIndex(r => r.ExpiresAt);
    }
}
