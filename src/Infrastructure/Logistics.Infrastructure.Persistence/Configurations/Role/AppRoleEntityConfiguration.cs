using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AppRoleEntityConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.HasMany(i => i.Claims)
            .WithOne(i => i.Role)
            .HasForeignKey(i => i.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
