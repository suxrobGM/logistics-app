using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TenantTaxRateEntityConfiguration : IEntityTypeConfiguration<TenantTaxRate>
{
    public void Configure(EntityTypeBuilder<TenantTaxRate> builder)
    {
        builder.ToTable("tenant_tax_rates");

        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ComplexProperty(r => r.Jurisdiction, j =>
        {
            j.Property(p => p.CountryCode).HasMaxLength(2).IsRequired();
            j.Property(p => p.Region).HasMaxLength(10);
        });

        builder.Property(r => r.RatePercent).HasPrecision(5, 2);
        builder.Property(r => r.TaxCode).HasMaxLength(50);
        builder.Property(r => r.Description).HasMaxLength(200);

        builder.HasIndex(r => new { r.TenantId, r.EffectiveFrom });
    }
}
