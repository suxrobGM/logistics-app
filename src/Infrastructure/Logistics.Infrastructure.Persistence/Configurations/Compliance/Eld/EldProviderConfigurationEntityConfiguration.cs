using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class EldProviderConfigurationEntityConfiguration : IEntityTypeConfiguration<EldProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<EldProviderConfiguration> builder)
    {
        builder.ToTable("eld_provider_configurations");

        builder.HasIndex(i => i.ProviderType)
            .IsUnique();

        // Secret columns are encrypted at rest (EncryptedStringConverter); ciphertext is larger
        // than the plaintext, so these are stored as unbounded text to avoid truncation.
        builder.Property(i => i.ApiKey)
            .IsRequired();

        builder.Property(i => i.ExternalAccountId)
            .HasMaxLength(100);

        builder.Property(i => i.Region)
            .HasMaxLength(10);
    }
}
