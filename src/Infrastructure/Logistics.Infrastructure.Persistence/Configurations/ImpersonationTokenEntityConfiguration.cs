using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ImpersonationTokenEntityConfiguration : IEntityTypeConfiguration<ImpersonationToken>
{
    public void Configure(EntityTypeBuilder<ImpersonationToken> builder)
    {
        builder.ToTable("impersonation_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.ExpiresAt);
        builder.HasIndex(t => new { t.IsUsed, t.ExpiresAt });
    }
}
