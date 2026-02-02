using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadBoardConfigurationEntityConfiguration : IEntityTypeConfiguration<LoadBoardConfiguration>
{
    public void Configure(EntityTypeBuilder<LoadBoardConfiguration> builder)
    {
        builder.ToTable("LoadBoardConfigurations");

        builder.HasIndex(i => i.ProviderType)
            .IsUnique();

        builder.Property(i => i.ApiKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.ApiSecret)
            .HasMaxLength(500);

        builder.Property(i => i.AccessToken)
            .HasMaxLength(2000);

        builder.Property(i => i.RefreshToken)
            .HasMaxLength(2000);

        builder.Property(i => i.WebhookSecret)
            .HasMaxLength(500);

        builder.Property(i => i.ExternalAccountId)
            .HasMaxLength(100);

        builder.Property(i => i.CompanyDotNumber)
            .HasMaxLength(20);

        builder.Property(i => i.CompanyMcNumber)
            .HasMaxLength(20);
    }
}
