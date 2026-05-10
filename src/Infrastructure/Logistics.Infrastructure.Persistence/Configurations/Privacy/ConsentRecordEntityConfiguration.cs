using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations.Privacy;

internal sealed class ConsentRecordEntityConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("consent_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.IpAddress).HasMaxLength(64);
        builder.Property(r => r.UserAgent).HasMaxLength(512);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.ConsentType, r.Timestamp });
    }
}
