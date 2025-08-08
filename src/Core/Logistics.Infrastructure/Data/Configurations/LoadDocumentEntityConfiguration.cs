using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

public class LoadDocumentEntityConfiguration : IEntityTypeConfiguration<LoadDocument>
{
    public void Configure(EntityTypeBuilder<LoadDocument> builder)
    {
        builder.ToTable("LoadDocuments");

        // Primary key
        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.BlobPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.BlobContainer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Shared.Consts.DocumentStatus.Active);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.UpdatedAt);

        // Relationships
        builder.HasOne(d => d.Load)
            .WithMany(l => l.Documents)
            .HasForeignKey(d => d.LoadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(d => d.LoadId);
        builder.HasIndex(d => d.UploadedById);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.CreatedAt);

        // Composite index for efficient queries
        builder.HasIndex(d => new { d.LoadId, d.Status, d.Type });
    }
}