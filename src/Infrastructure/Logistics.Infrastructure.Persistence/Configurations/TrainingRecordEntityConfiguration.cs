using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TrainingRecordEntityConfiguration : IEntityTypeConfiguration<TrainingRecord>
{
    public void Configure(EntityTypeBuilder<TrainingRecord> builder)
    {
        builder.ToTable("TrainingRecords");

        builder.HasIndex(i => new { i.EmployeeId, i.TrainingType });
        builder.HasIndex(i => i.CompletedDate);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.TrainingName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Provider)
            .HasMaxLength(200);

        builder.Property(i => i.CertificateNumber)
            .HasMaxLength(100);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);
    }
}
