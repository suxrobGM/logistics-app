using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class InboundEmailRouteEntityConfiguration : IEntityTypeConfiguration<InboundEmailRoute>
{
    public void Configure(EntityTypeBuilder<InboundEmailRoute> builder)
    {
        builder.ToTable("inbound_email_routes");

        builder.Property(x => x.ThreadToken).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.ThreadToken).IsUnique();
        builder.HasIndex(x => x.TenantId);
    }
}
