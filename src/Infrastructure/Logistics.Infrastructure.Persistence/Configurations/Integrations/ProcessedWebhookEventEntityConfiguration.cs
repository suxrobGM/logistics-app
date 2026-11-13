using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedWebhookEventEntityConfiguration : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable("processed_webhook_events");

        builder.Property(x => x.Provider).HasMaxLength(32).IsRequired();
        builder.Property(x => x.EventKey).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => new { x.Provider, x.EventKey }).IsUnique();
        builder.HasIndex(x => x.ReceivedAt);
    }
}
