using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AiDispatchDecisionEntityConfiguration : IEntityTypeConfiguration<AiDispatchDecision>
{
    public void Configure(EntityTypeBuilder<AiDispatchDecision> builder)
    {
        builder.ToTable("ai_dispatch_decisions");

        builder.Property(d => d.Reasoning)
            .HasMaxLength(4000);

        builder.Property(d => d.ToolName)
            .HasMaxLength(100);

        builder.Property(d => d.RejectionReason)
            .HasMaxLength(1000);

        builder.HasIndex(d => d.SessionId);
        builder.HasIndex(d => d.Status);
    }
}
