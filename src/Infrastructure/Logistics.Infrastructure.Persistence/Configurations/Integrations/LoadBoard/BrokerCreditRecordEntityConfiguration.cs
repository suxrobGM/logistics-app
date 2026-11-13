using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class BrokerCreditRecordEntityConfiguration : IEntityTypeConfiguration<BrokerCreditRecord>
{
    public void Configure(EntityTypeBuilder<BrokerCreditRecord> builder)
    {
        builder.ToTable("broker_credit_records");

        builder.HasIndex(i => i.McNumber)
            .IsUnique();

        builder.Property(i => i.McNumber)
            .HasMaxLength(20)
            .IsRequired();
    }
}
