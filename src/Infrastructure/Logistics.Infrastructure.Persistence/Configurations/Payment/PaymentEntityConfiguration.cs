using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        // NOTE (audit finding #19 - optimistic concurrency): payments are money, so a concurrent
        // read-modify-write can silently lose an update. The intended fix is a Postgres xmin
        // concurrency token, but Npgsql 10 removed UseXminAsConcurrencyToken, and hand-rolling the
        // shadow property makes EF try to CREATE the xmin system column in a migration (it already
        // exists on every table), which would break schema updates. Deliberately left unimplemented
        // rather than shipping a fragile mechanism on the money path - see the audit report for the
        // recommended approach.

        builder.ComplexProperty(i => i.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });
    }
}
