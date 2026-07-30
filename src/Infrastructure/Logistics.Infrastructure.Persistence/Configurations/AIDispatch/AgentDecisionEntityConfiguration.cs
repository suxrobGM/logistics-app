using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AgentDecisionEntityConfiguration : IEntityTypeConfiguration<AgentDecision>
{
    public void Configure(EntityTypeBuilder<AgentDecision> builder)
    {
        builder.ToTable("agent_decisions");

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
