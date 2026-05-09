using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations.Privacy;

internal sealed class DataDeletionRequestEntityConfiguration : IEntityTypeConfiguration<DataDeletionRequest>
{
    public void Configure(EntityTypeBuilder<DataDeletionRequest> builder)
    {
        builder.ToTable("data_deletion_requests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason).HasMaxLength(2048);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ScheduledFor);
    }
}
