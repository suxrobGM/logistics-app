using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ApiKeyEntityConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(k => k.KeyHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(k => k.KeyPrefix)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(k => k.KeyHash).IsUnique();
        builder.HasIndex(k => k.IsActive);
    }
}
