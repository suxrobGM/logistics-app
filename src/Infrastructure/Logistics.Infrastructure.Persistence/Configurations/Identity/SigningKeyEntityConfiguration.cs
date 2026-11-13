using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class SigningKeyEntityConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.ToTable("signing_keys");

        builder.Property(x => x.Algorithm).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Data).IsRequired();

        builder.HasIndex(x => x.Created);
    }
}
