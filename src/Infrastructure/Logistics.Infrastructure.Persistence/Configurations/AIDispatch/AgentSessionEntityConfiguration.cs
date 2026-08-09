using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AgentSessionEntityConfiguration : IEntityTypeConfiguration<AgentSession>
{
    public void Configure(EntityTypeBuilder<AgentSession> builder)
    {
        builder.ToTable("agent_sessions");

        builder.Ignore(s => s.TotalTokensUsed);

        builder.Property(s => s.Summary)
            .HasMaxLength(4000);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasMany(s => s.Decisions)
            .WithOne(d => d.Session)
            .HasForeignKey(d => d.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.ConversationId);

        // The quota check sums this week's cost before every dispatch run and copilot turn.
        // Covering, so that sum never leaves the index.
        builder.HasIndex(s => s.StartedAt)
            .IncludeProperties(s => s.EstimatedCostUsd);

        // A conversation is the user's own audit surface, so deleting it takes its turn sessions
        // (and transitively their decisions) with it.
        builder.HasOne(s => s.Conversation)
            .WithMany()
            .HasForeignKey(s => s.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
