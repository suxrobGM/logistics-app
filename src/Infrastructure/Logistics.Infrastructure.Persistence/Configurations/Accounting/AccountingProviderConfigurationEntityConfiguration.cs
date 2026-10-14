using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AccountingProviderConfigurationEntityConfiguration
    : IEntityTypeConfiguration<AccountingProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<AccountingProviderConfiguration> builder)
    {
        builder.ToTable("accounting_provider_configurations");

        builder.HasIndex(i => i.ProviderType)
            .IsUnique();

        builder.Property(i => i.RealmId)
            .HasMaxLength(100);

        builder.Property(i => i.CompanyName)
            .HasMaxLength(256);

        builder.Property(i => i.DefaultPaymentAccountId)
            .HasMaxLength(100);

        builder.Property(i => i.DefaultPaymentAccountType)
            .HasMaxLength(50);

        builder.Property(i => i.DefaultExpenseAccountId)
            .HasMaxLength(100);

        builder.Property(i => i.DefaultItemId)
            .HasMaxLength(100);

        builder.Property(i => i.DepositAccountId)
            .HasMaxLength(100);

        builder.Property(i => i.PayrollExpenseAccountId)
            .HasMaxLength(100);

        builder.Property(i => i.MappingsJson)
            .HasColumnType("jsonb");

        // AccessToken / RefreshToken are encrypted at rest (EncryptedStringConverter) and stored
        // as unbounded text - ciphertext is larger than the plaintext token.
    }
}
