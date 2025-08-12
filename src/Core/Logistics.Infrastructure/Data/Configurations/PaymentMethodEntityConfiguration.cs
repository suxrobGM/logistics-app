using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

public class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods")
            .HasDiscriminator(pm => pm.Type)
            .HasValue<CardPaymentMethod>(PaymentMethodType.Card)
            .HasValue<UsBankAccountPaymentMethod>(PaymentMethodType.UsBankAccount)
            .HasValue<BankAccountPaymentMethod>(PaymentMethodType.InternationalBankAccount);

        builder.HasIndex(i => i.StripePaymentMethodId)
            .IsUnique();
    }
}
