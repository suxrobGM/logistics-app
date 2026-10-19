using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadBoardConfigurationEntityConfiguration : IEntityTypeConfiguration<LoadBoardConfiguration>
{
    public void Configure(EntityTypeBuilder<LoadBoardConfiguration> builder)
    {
        builder.ToTable("load_board_configurations");

        builder.HasIndex(i => i.ProviderType)
            .IsUnique();

        // Secret columns are encrypted at rest (EncryptedStringConverter); ciphertext is larger
        // than the plaintext, so these are stored as unbounded text to avoid truncation.
        builder.Property(i => i.ApiKey)
            .IsRequired();

        builder.Property(i => i.UserEmail)
            .HasMaxLength(256);

        builder.Property(i => i.CompanyDotNumber)
            .HasMaxLength(20);

        builder.Property(i => i.CompanyMcNumber)
            .HasMaxLength(20);
    }
}
