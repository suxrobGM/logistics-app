using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AgentConversationEntityConfiguration : IEntityTypeConfiguration<AgentConversation>
{
    public void Configure(EntityTypeBuilder<AgentConversation> builder)
    {
        builder.ToTable("agent_conversations");

        builder.Property(c => c.Title)
            .HasMaxLength(120);

        builder.HasIndex(c => c.CreatedById);

        // The message sequence counter. Only the reservation statement in AgentMessageSequences reads or
        // advances it, so no tracked copy can write a stale value back.
        builder.Property<int>("LastSequence");

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
