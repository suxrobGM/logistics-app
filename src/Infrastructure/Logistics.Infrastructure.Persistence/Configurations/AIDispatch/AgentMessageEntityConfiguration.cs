using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AgentMessageEntityConfiguration : IEntityTypeConfiguration<AgentMessage>
{
    public void Configure(EntityTypeBuilder<AgentMessage> builder)
    {
        builder.ToTable("agent_messages");

        builder.Property(m => m.DisplayText)
            .HasMaxLength(4000);

        builder.HasIndex(m => new { m.ConversationId, m.Sequence })
            .IsUnique();
    }
}
