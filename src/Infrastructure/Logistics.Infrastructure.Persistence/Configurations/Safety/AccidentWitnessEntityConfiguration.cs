using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AccidentWitnessEntityConfiguration : IEntityTypeConfiguration<AccidentWitness>
{
    public void Configure(EntityTypeBuilder<AccidentWitness> builder)
    {
        builder.ToTable("accident_witnesses");

        builder.HasIndex(i => i.AccidentReportId);

        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.Property(i => i.PhoneNumber).HasMaxLength(50);
        builder.Property(i => i.Email).HasMaxLength(200);
        builder.Property(i => i.Address).HasMaxLength(500);
        builder.Property(i => i.Statement).HasMaxLength(4000);
    }
}
