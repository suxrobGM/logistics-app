using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AICopilotConversationEntityConfiguration : IEntityTypeConfiguration<AICopilotConversation>
{
    public void Configure(EntityTypeBuilder<AICopilotConversation> builder)
    {
        builder.ToTable("ai_copilot_conversations");

        builder.Property(c => c.Title)
            .HasMaxLength(120);

        builder.HasIndex(c => c.CreatedById);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
