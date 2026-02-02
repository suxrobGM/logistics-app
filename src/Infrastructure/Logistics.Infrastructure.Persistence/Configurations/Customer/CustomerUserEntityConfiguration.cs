using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class CustomerUserEntityConfiguration : IEntityTypeConfiguration<CustomerUser>
{
    public void Configure(EntityTypeBuilder<CustomerUser> builder)
    {
        builder.ToTable("CustomerUsers");

        builder.HasIndex(cu => cu.UserId);
        builder.HasIndex(cu => cu.Email);
        builder.HasIndex(cu => new { cu.CustomerId, cu.UserId }).IsUnique();

        builder.Property(cu => cu.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(cu => cu.DisplayName)
            .HasMaxLength(256);

        builder.HasOne(cu => cu.Customer)
            .WithMany()
            .HasForeignKey(cu => cu.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
