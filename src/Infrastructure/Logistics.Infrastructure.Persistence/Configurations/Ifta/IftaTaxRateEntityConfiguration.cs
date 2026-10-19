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

        // Row identity (year, quarter, jurisdiction) is the raw-SQL unique index
        // ux_ifta_tax_rates_year_quarter_jurisdiction (NULLS NOT DISTINCT on region), created in the migration
        // because complex-type members can't be in an EF index. IftaTaxRateUniqueness still probes first
        // so a duplicate gets a readable error.
        builder.HasIndex(r => new { r.Year, r.Quarter });
    }
}
