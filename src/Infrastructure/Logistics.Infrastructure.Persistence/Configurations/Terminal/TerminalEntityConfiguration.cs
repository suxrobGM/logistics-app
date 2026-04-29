using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TerminalEntityConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("terminals");

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Code)
            .HasMaxLength(5)
            .IsRequired();

        builder.HasIndex(t => t.Code).IsUnique();

        builder.Property(t => t.CountryCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.Notes).HasMaxLength(2000);
    }
}
