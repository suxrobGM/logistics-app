using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AIDispatchSessionEntityConfiguration : IEntityTypeConfiguration<AIDispatchSession>
{
    public void Configure(EntityTypeBuilder<AIDispatchSession> builder)
    {
        builder.ToTable("ai_dispatch_sessions");

        builder.Property(s => s.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(s => s.Number)
            .IsUnique();

        builder.Ignore(s => s.TotalTokensUsed);

        builder.Property(s => s.RequestCost)
            .HasDefaultValue(1);

        builder.Property(s => s.Summary)
            .HasMaxLength(4000);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasMany(s => s.Decisions)
            .WithOne(d => d.Session)
            .HasForeignKey(d => d.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.ConversationId);

        // Deleting a conversation removes its turn sessions (and, transitively, their decisions) -
        // the conversation is the user's own audit surface.
        builder.HasOne(s => s.Conversation)
            .WithMany()
            .HasForeignKey(s => s.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
