using Logistics.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class TenantRoleClaimEntityConfiguration : IEntityTypeConfiguration<TenantRoleClaim>
{
    public void Configure(EntityTypeBuilder<TenantRoleClaim> builder)
    {
        builder.ToTable("RoleClaims");
    }
}
