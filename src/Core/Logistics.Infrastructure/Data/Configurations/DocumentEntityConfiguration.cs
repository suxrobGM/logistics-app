using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class DocumentEntityConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasDiscriminator(d => d.OwnerType)
            .HasValue<EmployeeDocument>(DocumentOwnerType.Employee)
            .HasValue<LoadDocument>(DocumentOwnerType.Load);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.BlobPath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(d => d.BlobContainer)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(DocumentStatus.Active);

        builder.Property(d => d.OwnerType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        // Relations
        builder.HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }

    #region Derived Types Configuration

    public sealed class LoadDocumentConfiguration : IEntityTypeConfiguration<LoadDocument>
    {
        public void Configure(EntityTypeBuilder<LoadDocument> builder)
        {
            builder.HasOne(ld => ld.Load)
                .WithMany(l => l.Documents)
                .HasForeignKey(ld => ld.LoadId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ld => ld.LoadId);
        }
    }

    public sealed class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
    {
        public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
        {
            builder.HasOne(ed => ed.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(ed => ed.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ed => ed.EmployeeId);
        }
    }

    #endregion
}
