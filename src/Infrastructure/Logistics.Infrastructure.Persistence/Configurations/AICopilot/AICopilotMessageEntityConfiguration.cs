using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AICopilotMessageEntityConfiguration : IEntityTypeConfiguration<AICopilotMessage>
{
    public void Configure(EntityTypeBuilder<AICopilotMessage> builder)
    {
        builder.ToTable("ai_copilot_messages");

        builder.Property(m => m.DisplayText)
            .HasMaxLength(4000);

        builder.HasIndex(m => new { m.ConversationId, m.Sequence })
            .IsUnique();
    }
}
