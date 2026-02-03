using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DocumentEntityConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasDiscriminator(d => d.OwnerType)
            .HasValue<EmployeeDocument>(DocumentOwnerType.Employee)
            .HasValue<LoadDocument>(DocumentOwnerType.Load)
            .HasValue<DeliveryDocument>(DocumentOwnerType.Delivery)
            .HasValue<TruckDocument>(DocumentOwnerType.Truck);

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
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasDefaultValue(DocumentStatus.Active);

        builder.Property(d => d.OwnerType)
            .IsRequired();

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

    public sealed class DeliveryDocumentConfiguration : IEntityTypeConfiguration<DeliveryDocument>
    {
        public void Configure(EntityTypeBuilder<DeliveryDocument> builder)
        {
            // POD/BOL capture metadata
            builder.Property(d => d.RecipientName)
                .HasMaxLength(255);

            builder.Property(d => d.RecipientSignature)
                .HasMaxLength(2048);

            builder.Property(d => d.CaptureLatitude);
            builder.Property(d => d.CaptureLongitude);
            builder.Property(d => d.CapturedAt);
            builder.Property(d => d.TripStopId);

            builder.Property(d => d.Notes)
                .HasMaxLength(2000);

            // TripStop relation
            builder.HasOne(d => d.TripStop)
                .WithMany()
                .HasForeignKey(d => d.TripStopId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public sealed class TruckDocumentConfiguration : IEntityTypeConfiguration<TruckDocument>
    {
        public void Configure(EntityTypeBuilder<TruckDocument> builder)
        {
            builder.HasOne(td => td.Truck)
                .WithMany(t => t.Documents)
                .HasForeignKey(td => td.TruckId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(td => td.TruckId);
        }
    }

    #endregion
}
