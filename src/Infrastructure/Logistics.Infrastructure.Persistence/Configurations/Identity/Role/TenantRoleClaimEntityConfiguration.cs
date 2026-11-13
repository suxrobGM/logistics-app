using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TenantRoleClaimEntityConfiguration : IEntityTypeConfiguration<TenantRoleClaim>
{
    public void Configure(EntityTypeBuilder<TenantRoleClaim> builder) => builder.ToTable("tenant_role_claims");
}
