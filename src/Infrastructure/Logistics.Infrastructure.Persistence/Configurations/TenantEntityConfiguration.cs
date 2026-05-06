using Logistics.Domain.Entities;
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
    }
}
