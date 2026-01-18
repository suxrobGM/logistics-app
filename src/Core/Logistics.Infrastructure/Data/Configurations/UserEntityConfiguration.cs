using Logistics.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasOne(i => i.Tenant)
            .WithMany(i => i.Users)
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
