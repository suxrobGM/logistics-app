using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ImpersonationAuditLogEntityConfiguration : IEntityTypeConfiguration<ImpersonationAuditLog>
{
    public void Configure(EntityTypeBuilder<ImpersonationAuditLog> builder)
    {
        builder.ToTable("impersonation_audit_logs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.AdminEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.TargetEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.IpAddress)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(l => l.UserAgent)
            .HasMaxLength(512);

        builder.Property(l => l.FailureReason)
            .HasMaxLength(512);

        builder.HasIndex(l => l.AdminUserId);
        builder.HasIndex(l => l.TargetUserId);
        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => new { l.WasSuccessful, l.Timestamp });
    }
}
