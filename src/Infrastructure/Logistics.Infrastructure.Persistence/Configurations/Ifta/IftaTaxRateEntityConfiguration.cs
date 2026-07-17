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

        // Row identity (year, quarter, jurisdiction) is enforced by the seeder/upsert —
        // complex-type members can't participate in a unique index.
        builder.HasIndex(r => new { r.Year, r.Quarter });
    }
}
