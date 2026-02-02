using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");

        builder.HasDiscriminator(pm => pm.Type)
            .HasValue<CardPaymentMethod>(PaymentMethodType.Card)
            .HasValue<UsBankAccountPaymentMethod>(PaymentMethodType.UsBankAccount)
            .HasValue<BankAccountPaymentMethod>(PaymentMethodType.InternationalBankAccount);

        builder.HasIndex(i => i.StripePaymentMethodId)
            .IsUnique();
    }
}
