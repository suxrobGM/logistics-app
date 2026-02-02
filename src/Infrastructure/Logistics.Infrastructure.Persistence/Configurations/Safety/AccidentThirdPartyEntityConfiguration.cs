using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AccidentThirdPartyEntityConfiguration : IEntityTypeConfiguration<AccidentThirdParty>
{
    public void Configure(EntityTypeBuilder<AccidentThirdParty> builder)
    {
        builder.ToTable("AccidentThirdParties");

        builder.HasIndex(i => i.AccidentReportId);

        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.Property(i => i.PhoneNumber).HasMaxLength(50);
        builder.Property(i => i.Address).HasMaxLength(500);
        builder.Property(i => i.DriverLicense).HasMaxLength(50);
        builder.Property(i => i.VehicleMake).HasMaxLength(100);
        builder.Property(i => i.VehicleModel).HasMaxLength(100);
        builder.Property(i => i.VehicleLicensePlate).HasMaxLength(20);
        builder.Property(i => i.VehicleVin).HasMaxLength(20);
        builder.Property(i => i.VehicleColor).HasMaxLength(50);
        builder.Property(i => i.InsuranceCompany).HasMaxLength(200);
        builder.Property(i => i.InsurancePolicyNumber).HasMaxLength(100);
        builder.Property(i => i.InsuranceAgentPhone).HasMaxLength(50);
    }
}
