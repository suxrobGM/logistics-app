using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class UserTenantAccessEntityConfiguration : IEntityTypeConfiguration<UserTenantAccess>
{
    public void Configure(EntityTypeBuilder<UserTenantAccess> builder)
    {
        builder.ToTable("UserTenantAccess");

        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => new { i.UserId, i.TenantId }).IsUnique();

        builder.Property(i => i.CustomerName)
            .HasMaxLength(255);

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
