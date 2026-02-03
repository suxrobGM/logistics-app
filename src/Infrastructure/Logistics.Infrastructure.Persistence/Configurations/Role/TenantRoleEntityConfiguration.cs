using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TenantRoleEntityConfiguration : IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> builder)
    {
        builder.ToTable("tenant_roles");

        builder.HasMany(i => i.Claims)
            .WithOne(i => i.Role)
            .HasForeignKey(i => i.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
