using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TenantEntityConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.Property(t => t.McNumber).HasMaxLength(20);
        builder.Property(t => t.VatNumber).HasMaxLength(20);
        builder.Property(t => t.EoriNumber).HasMaxLength(20);
        builder.Property(t => t.CompanyRegistrationNumber).HasMaxLength(50);
        builder.Property(t => t.TaxResidencyCountry).HasMaxLength(2);

        // SnakeCaseEnumConvention skips collection elements, so the converter is set here.
        builder.PrimitiveCollection(t => t.Presets)
            .ElementType(e => e.HasConversion<SnakeCaseEnumConverter<TenantPreset>>());

        builder.ComplexProperty(t => t.Settings, settings =>
        {
            settings.Property(s => s.DefaultRateFloorPerMile).HasPrecision(18, 2);
        });
    }
}
