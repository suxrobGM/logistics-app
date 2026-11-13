using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class IftaTaxRateEntityConfiguration : IEntityTypeConfiguration<IftaTaxRate>
{
    public void Configure(EntityTypeBuilder<IftaTaxRate> builder)
    {
        builder.ToTable("ifta_tax_rates");

        builder.ComplexProperty(r => r.Jurisdiction, j =>
        {
            j.Property(p => p.CountryCode).HasMaxLength(2).IsRequired();
            j.Property(p => p.Region).HasMaxLength(10);
        });

        builder.Property(r => r.RatePerGallon).HasPrecision(8, 4);
        builder.Property(r => r.SurchargeRatePerGallon).HasPrecision(8, 4);

        // The row's unique index is created with raw SQL in the migration, since EF cannot index complex-type members.
    }
}
