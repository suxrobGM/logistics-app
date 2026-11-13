using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class FactoringCompanyEntityConfiguration : IEntityTypeConfiguration<FactoringCompany>
{
    public void Configure(EntityTypeBuilder<FactoringCompany> builder)
    {
        builder.ToTable("factoring_companies");

        builder.Property(f => f.Name).HasMaxLength(200).IsRequired();
        builder.Property(f => f.SubmissionEmail).HasMaxLength(320).IsRequired();
        builder.Property(f => f.Phone).HasMaxLength(50);
        builder.Property(f => f.AdvanceRatePercent).HasPrecision(5, 2);
        builder.Property(f => f.FeePercent).HasPrecision(5, 2);
        builder.Ignore(f => f.ReservePercent);

        builder.HasMany<Customer>()
            .WithOne(c => c.FactoringCompany)
            .HasForeignKey(c => c.FactoringCompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
